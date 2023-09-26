using IdentityServer4.Configuration;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Validation;
using IdentityServer4.Extensions;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Stores;
using IdentityServer4.Models;

namespace IdentityServer4.Endpoints.Results
{
    public class ConsentPageResult2 : IEndpointResult
    {
        private readonly IdentityServerOptions _options;
        private readonly Guid _consentRequestId;

        public ConsentPageResult2(IdentityServerOptions options, Guid consentRequestId)
        {
            _options = options;
            _consentRequestId = consentRequestId;
        }

        public Task ExecuteAsync(HttpContext context)
        {
            var consentUrl = _options.UserInteraction.ConsentUrl;
            var redirectUrl = consentUrl.AddQueryString("consentRequestId", _consentRequestId.ToString("N"));
            context.Response.RedirectToAbsoluteUrl(redirectUrl);
            return Task.CompletedTask;
        }
    }
}