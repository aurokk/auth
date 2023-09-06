// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Endpoints
{
    internal class AuthorizeCallbackEndpoint : AuthorizeEndpointBase
    {
        private readonly ILoginResponseMessageStore _loginResponseMessageStore;
        private readonly IConsentResponseMessageStore _consentResponseResponseStore;
        private readonly IAuthorizationParametersMessageStore _authorizationParametersMessageStore;
        private readonly ILoginResponseIdToRequestIdMessageStore _loginResponseIdToRequestIdMessageStore;

        public AuthorizeCallbackEndpoint(
            IEventService events,
            ILogger<AuthorizeCallbackEndpoint> logger,
            IdentityServerOptions options,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizeResponseGenerator authorizeResponseGenerator,
            IUserSession userSession,
            IConsentResponseMessageStore consentResponseResponseStore,
            ILoginResponseMessageStore loginResponseMessageStore,
            ILoginResponseIdToRequestIdMessageStore loginResponseIdToRequestIdMessageStore,
            IAuthorizationParametersMessageStore authorizationParametersMessageStore = null)
            : base(events, logger, options, validator, interactionGenerator, authorizeResponseGenerator, userSession)
        {
            _consentResponseResponseStore = consentResponseResponseStore;
            _loginResponseMessageStore = loginResponseMessageStore;
            _loginResponseIdToRequestIdMessageStore = loginResponseIdToRequestIdMessageStore;
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

            var parameters = context.Request.Query.AsNameValueCollection();
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

            var loginResponseId = parameters["loginResponseId"];
            if (user == null)
            {
                // TODO: переписать нормально
                if (loginResponseId == null)
                {
                    return await CreateErrorResultAsync("missing loginResponseId");
                }

                var loginResponseIdToRequestIdMessage =
                    await _loginResponseIdToRequestIdMessageStore.ReadAsync(loginResponseId);
                if (loginResponseIdToRequestIdMessage == null)
                {
                    return await CreateErrorResultAsync("unknown loginResponseId");
                }

                var loginResponseMessage = await _loginResponseMessageStore.ReadAsync(loginResponseId);
                if (loginResponseMessage == null)
                {
                    return await CreateErrorResultAsync("missing loginResponseId");
                }

                {
                    var identityServerUser = new IdentityServerUser(loginResponseMessage.Data.SubjectId)
                    {
                        IdentityProvider = "identity", // TODO
                        AuthenticationTime =  DateTime.UtcNow, // TODO
                    };
                    await context.SignInAsync(identityServerUser, new AuthenticationProperties { IsPersistent = true });
                    context.User = identityServerUser.CreatePrincipal();
                    user = context.User;
                }
            }

            var consentRequest = new ConsentRequest(parameters, user?.GetSubjectId());
            var consentResult = await _consentResponseResponseStore.ReadAsync(consentRequest.Id);
            if (consentResult is { Data: null })
            {
                return await CreateErrorResultAsync("consent message is missing data");
            }

            try
            {
                var result = await ProcessAuthorizeRequestAsync(parameters, user, consentResult?.Data, loginResponseId);

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