using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity;
using IdentityModel;
using IdentityServer4;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Users.Logins;

[PublicAPI]
public sealed record UnlinkRequest(string Provider);

[PublicAPI]
public sealed record LinkRequest(string Provider);

[PublicAPI]
public sealed record LinkAppleRequest(string IdToken);

[Authorize("User")]
[ApiController]
[Route("api/users/logins")]
public class LoginsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginsController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [ProducesResponseType(200)]
    [Route("unlink")]
    [HttpPost]
    public async Task<IActionResult> Unlink(UnlinkRequest request, CancellationToken ct)
    {
        var userIdClaim = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userIdClaim.Value);
        if (user is null)
            return BadRequest();

        var logins = await _userManager.GetLoginsAsync(user);

        var methodsCount = 0;
        methodsCount += user.Email != null ? 1 : 0;
        methodsCount += logins.Count;

        if (methodsCount == 1)
            return BadRequest();

        switch (request)
        {
            case { Provider: "google" }:
            {
                var googleLogins = logins.Where(x => x.LoginProvider == "Google").ToArray();
                foreach (var googleLogin in googleLogins)
                    await _userManager.RemoveLoginAsync(user, googleLogin.LoginProvider, googleLogin.ProviderKey);
                return Ok();
            }

            case { Provider: "apple" }:
            {
                var appleLogins = logins.Where(x => x.LoginProvider == "Apple").ToArray();
                foreach (var appleLogin in appleLogins)
                    await _userManager.RemoveLoginAsync(user, appleLogin.LoginProvider, appleLogin.ProviderKey);
                return Ok();
            }

            case { Provider: "email" }:
            {
                await _userManager.SetEmailAsync(user, null);
                return Ok();
            }

            default:
            {
                return BadRequest();
            }
        }
    }

    [ProducesResponseType(200)]
    [Route("link/applenative")]
    [HttpPost]
    public async Task<IActionResult> LinkAppleNative(LinkAppleRequest request, CancellationToken ct)
    {
        var userIdClaim = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userIdClaim.Value);
        if (user is null)
            return BadRequest();

        var tokenHandler = new JwtSecurityTokenHandler();
        using var client = new HttpClient();
        var response = await client.GetFromJsonAsync<Response>(
            "https://appleid.apple.com/auth/keys",
            cancellationToken: ct
        );
        var keys = response?.Keys ?? throw new ApplicationException();
        var claimsPrincipal = tokenHandler.ValidateToken(
            request.IdToken,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new List<string> { "https://appleid.apple.com" },
                ValidateAudience = true,
                ValidAudiences = new List<string> { "com.dmitriikochnev.words" },
                ValidateLifetime = true,
                IssuerSigningKeys = keys,
            },
            out _
        );
        var provider = "Apple"; // TODO: review
        var providerUserId = claimsPrincipal.FindFirst(JwtClaimTypes.Subject) ??
                             claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) ??
                             throw new Exception("Unknown userid");
        await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId.Value, provider));
        return Ok();
    }

    [ProducesResponseType(200)]
    [Route("link")]
    [HttpGet]
    public async Task<IActionResult> Link([FromQuery] LinkRequest request, CancellationToken ct)
    {
        switch (request.Provider)
        {
            case "google":
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                var requestHost = new Uri($"{Request.Scheme}://{Request.Host}");
                var redirectUrl = new Uri(requestHost,
                    $"api/users/logins/linkcallback?access_token={Request.Query["access_token"]}");
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                    provider: "Google",
                    redirectUrl: redirectUrl.ToString(),
                    userId: _userManager.GetUserId(User)
                );
                return new ChallengeResult("Google", properties);
            }

            default:
            {
                throw new ApplicationException();
            }
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(200)]
    [Route("linkcallback")]
    [HttpGet]
    public async Task<IActionResult> LinkCallback(CancellationToken ct)
    {
        var userIdClaim = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userIdClaim.Value);
        if (user is null)
            return BadRequest();

        var authResult = await HttpContext.AuthenticateAsync(
            IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (!authResult.Succeeded)
            return BadRequest();

        var provider = "Google";
        var providerUserId = authResult.Principal.FindFirst(JwtClaimTypes.Subject) ??
                             authResult.Principal.FindFirst(ClaimTypes.NameIdentifier) ??
                             throw new Exception("Unknown userid");
        var result = await _userManager.AddLoginAsync(user,
            new UserLoginInfo(provider, providerUserId.Value, provider));

        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

        if (result.Succeeded)
        {
            return Redirect("vocabu://auth-callback");
        }
        else
        {
            return Ok("Error");
        }
    }
}