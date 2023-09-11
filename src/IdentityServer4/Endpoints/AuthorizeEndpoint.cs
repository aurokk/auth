// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Net;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
            ILoginResponseIdToRequestIdMessageStore loginResponseIdToRequestIdMessageStore)
            : base(events, logger, options, validator, interactionGenerator, authorizeResponseGenerator, userSession)
        {
            _loginRequestIdToResponseIdMessageStore = loginRequestIdToResponseIdMessageStore;
            _loginResponseIdToRequestIdMessageStore = loginResponseIdToRequestIdMessageStore;
        }

        public override async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            Logger.LogDebug("Start authorize request");

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
            var loginRequestId = Guid.NewGuid().ToString();

            if (user == null)
            {
                // TODO: переписать нормально

                var loginResponseId = Guid.NewGuid().ToString();

                var loginRequestIdToResponseId = new LoginRequestIdToResponseId { LoginResponseId = loginResponseId, };
                var loginResponseIdToRequestId = new LoginResponseIdToRequestId { LoginRequestId = loginRequestId, };

                var loginRequestIdToResponseIdMessage = new Message<LoginRequestIdToResponseId>(
                    loginRequestIdToResponseId, DateTime.UtcNow);
                var loginResponseIdToRequestIdMessage = new Message<LoginResponseIdToRequestId>(
                    loginResponseIdToRequestId, DateTime.UtcNow);

                await _loginRequestIdToResponseIdMessageStore.WriteAsync(
                    loginRequestId, loginRequestIdToResponseIdMessage);
                await _loginResponseIdToRequestIdMessageStore.WriteAsync(
                    loginResponseId, loginResponseIdToRequestIdMessage);
            }


            // Тут интересно то, что как бы логин проверяется (через куку), а консент не проверяется
            // Т.е. всегда будет редирект на консент, хотя он уже может быть
            var result = await ProcessAuthorizeRequestAsync(parameters, user, null, loginRequestId);

            Logger.LogTrace("End authorize request. result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }
    }
}