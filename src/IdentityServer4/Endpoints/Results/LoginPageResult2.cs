using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Endpoints.Results;

public class LoginPageResult2 : IEndpointResult
{
    private readonly IdentityServerOptions _options;
    private readonly Guid _loginRequestId;

    public LoginPageResult2(IdentityServerOptions options, Guid loginRequestId)
    {
        _options = options;
        _loginRequestId = loginRequestId;
    }

    public Task ExecuteAsync(HttpContext context)
    {
        var loginUrl = _options.UserInteraction.LoginUrl;
        var redirectUrl = loginUrl.AddQueryString("loginRequestId", _loginRequestId.ToString("N"));
        context.Response.RedirectToAbsoluteUrl(redirectUrl);
        return Task.CompletedTask;
    }
}