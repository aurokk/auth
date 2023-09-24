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
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

// TODO: почистить код
// TODO: удалить старый consent стор
// TODO: сделать отдельный стор для authorization request

namespace IdentityServer4.Endpoints
{
    internal class AuthorizeEndpoint : AuthorizeEndpointBase
    {
        private readonly IAuthorizeRequest2Store _authorizeRequestStore;

        public AuthorizeEndpoint(
            IEventService events,
            ILogger<AuthorizeEndpoint> logger,
            IdentityServerOptions options,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizeResponseGenerator authorizeResponseGenerator,
            IUserSession userSession,
            ILoginRequestStore loginRequestStore,
            IConsentRequest2Store consentRequestStore,
            ILoginResponseStore loginResponseStore,
            IAuthorizeRequest2Store authorizeRequestStore)
            : base(
                events: events,
                logger: logger,
                options: options,
                validator: validator,
                interactionGenerator: interactionGenerator,
                authorizeResponseGenerator: authorizeResponseGenerator,
                userSession: userSession,
                loginRequestStore: loginRequestStore,
                consentRequestStore: consentRequestStore,
                loginResponseStore: loginResponseStore
            )
        {
            _authorizeRequestStore = authorizeRequestStore;
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

            var authorizeRequest = new AuthorizeRequest2(
                Id: Guid.NewGuid(),
                Data: parameters.ToQueryString(),
                CreatedAtUtc: DateTime.UtcNow,
                RemoveAtUtc: DateTime.UtcNow + TimeSpan.FromHours(1)
            );
            await _authorizeRequestStore.Create(authorizeRequest, context.RequestAborted);

            var user = await UserSession.GetUserAsync();
            var result = await ProcessAuthorizeRequestAsync(authorizeRequest, parameters, user, null);

            Logger.LogTrace("End authorize request. result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }
    }
}