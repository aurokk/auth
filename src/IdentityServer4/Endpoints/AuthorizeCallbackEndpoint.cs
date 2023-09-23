// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Net;
using System.Web;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Storage.Stores;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Endpoints
{
    internal class AuthorizeCallbackEndpoint : AuthorizeEndpointBase
    {
        private readonly IConsentResponseMessageStore _consentResponseResponseStore;
        private readonly IAuthorizationParametersMessageStore _authorizationParametersMessageStore;
        private readonly ILoginRequestStore _loginRequestStore;
        private readonly ILoginResponseStore _loginResponseStore;

        public AuthorizeCallbackEndpoint(
            IEventService events,
            ILogger<AuthorizeCallbackEndpoint> logger,
            IdentityServerOptions options,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizeResponseGenerator authorizeResponseGenerator,
            IUserSession userSession,
            IConsentResponseMessageStore consentResponseResponseStore,
            ILoginRequestStore loginRequestStore, ILoginResponseStore loginResponseStore,
            IAuthorizationParametersMessageStore authorizationParametersMessageStore = null)
            : base(events, logger, options, validator, interactionGenerator, authorizeResponseGenerator, userSession,
                loginRequestStore)
        {
            _consentResponseResponseStore = consentResponseResponseStore;
            _loginRequestStore = loginRequestStore;
            _loginResponseStore = loginResponseStore;
            _authorizationParametersMessageStore = authorizationParametersMessageStore;
        }

        public override async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                Logger.LogWarning("Invalid HTTP method for authorize endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            Logger.LogDebug("Start authorize callback request");

            var query = context.Request.Query.AsNameValueCollection();
            var parameters = new NameValueCollection();
            if (query["loginResponseId"] != null)
            {
                if (!Guid.TryParse(query["loginResponseId"], out var loginResponseId))
                {
                    return new StatusCodeResult(HttpStatusCode.BadRequest);
                }

                var loginResponse = await _loginResponseStore.Get(loginResponseId, context.RequestAborted);
                var loginRequest = await _loginRequestStore.Get(loginResponse.LoginRequestId, context.RequestAborted);
                parameters = HttpUtility.ParseQueryString(loginRequest.Data);
            }

            if (query["consentResponseId"] != null)
            {
                if (!Guid.TryParse(query["consentResponseId"], out var loginResponseId))
                {
                    return new StatusCodeResult(HttpStatusCode.BadRequest);
                }

                // var loginResponse = await _loginResponseStore.Get(loginResponseId, context.RequestAborted);
                // var loginRequest = await _loginRequestStore.Get(loginResponse.LoginRequestId, context.RequestAborted);
                // parameters = HttpUtility.ParseQueryString(loginRequest.Data);
                throw new NotImplementedException();
            }

            if (query["loginResponseId"] == null && query["consentResponseId"] == null)
            {
                parameters = query;
            }

            // var parameters = _loginRequestStore.Get()
            if (_authorizationParametersMessageStore != null)
            {
                // TODO: поисследовать че за мессадж стор
                // никакой докуменатции нет, похоже это не работает, но идея, в целом, была неплохой
                var messageStoreId = parameters[Constants.AuthorizationParamsStore.MessageStoreIdParameterName];
                var entry = await _authorizationParametersMessageStore.ReadAsync(messageStoreId);
                parameters = entry?.Data.FromFullDictionary() ?? new NameValueCollection();
                await _authorizationParametersMessageStore.DeleteAsync(messageStoreId);
            }

            var user = await UserSession.GetUserAsync();
            if (user == null && query["loginResponseId"] != null)
            {
                if (!Guid.TryParse(query["loginResponseId"], out var loginResponseId))
                {
                    return new StatusCodeResult(HttpStatusCode.BadRequest);
                }

                var loginResponse = await _loginResponseStore.Get(loginResponseId, context.RequestAborted);

                var identityServerUser = new IdentityServerUser(loginResponse.SubjectId)
                {
                    IdentityProvider = "identity", // TODO
                    AuthenticationTime = DateTime.UtcNow, // TODO
                };
                await context.SignInAsync(identityServerUser, new AuthenticationProperties { IsPersistent = true });
                context.User = identityServerUser.CreatePrincipal();
                user = context.User;
            }

            var consentRequest = new ConsentRequest(parameters, user?.GetSubjectId());
            var consentResult = await _consentResponseResponseStore.ReadAsync(consentRequest.Id);
            if (consentResult is { Data: null })
            {
                return await CreateErrorResultAsync("consent message is missing data");
            }

            try
            {
                var result = await ProcessAuthorizeRequestAsync(parameters, user, consentResult?.Data);

                Logger.LogTrace("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

                return result;
            }
            finally
            {
                if (consentResult != null)
                {
                    await _consentResponseResponseStore.DeleteAsync(consentRequest.Id);
                }
            }
        }
    }
}