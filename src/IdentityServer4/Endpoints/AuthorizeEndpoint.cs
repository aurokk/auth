// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Net;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Storage.Stores;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

// TODO: сделать типизированныq стор consentRequest
// TODO: сохранить consentRequest по-настоящему
// TODO: сделать типизированныq стор consentResponse
// TODO: сохранить consentResponse по-настоящему
// TODO: удалить старые сторы

namespace IdentityServer4.Endpoints
{
    internal class AuthorizeEndpoint : AuthorizeEndpointBase
    {
        private readonly ILoginRequestIdToResponseIdMessageStore _loginRequestIdToResponseIdMessageStore;
        private readonly ILoginResponseIdToRequestIdMessageStore _loginResponseIdToRequestIdMessageStore;

        public AuthorizeEndpoint(
            IEventService events,
            ILogger<AuthorizeEndpoint> logger,
            IdentityServerOptions options,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizeResponseGenerator authorizeResponseGenerator,
            IUserSession userSession, ILoginRequestIdToResponseIdMessageStore loginRequestIdToResponseIdMessageStore,
            ILoginResponseIdToRequestIdMessageStore loginResponseIdToRequestIdMessageStore,
            ILoginRequestStore loginRequestStore)
            : base(events, logger, options, validator, interactionGenerator, authorizeResponseGenerator, userSession,
                loginRequestStore)
        {
            _loginRequestIdToResponseIdMessageStore = loginRequestIdToResponseIdMessageStore;
            _loginResponseIdToRequestIdMessageStore = loginResponseIdToRequestIdMessageStore;
        }

        public override async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            Logger.LogDebug("Start authorize request");

            // TODO:
            // – перенести парсинг параметров в отдельный класс
            NameValueCollection parameters;

            if (HttpMethods.IsGet(context.Request.Method))
            {
                parameters = context.Request.Query.AsNameValueCollection();
            }
            else if (HttpMethods.IsPost(context.Request.Method))
            {
                if (!context.Request.HasApplicationFormContentType())
                {
                    return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);
                }

                parameters = context.Request.Form.AsNameValueCollection();
            }
            else
            {
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            var user = await UserSession.GetUserAsync();

            var result = await ProcessAuthorizeRequestAsync(parameters, user, null);

            Logger.LogTrace("End authorize request. result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }
    }
}