using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace Api;

public record Response(JsonWebKey[] Keys);

[UsedImplicitly]
public class CustomSignInWithAppleGrantValidator : IExtensionGrantValidator
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomSignInWithAppleGrantValidator(UserManager<ApplicationUser> userManager) =>
        _userManager = userManager;

    public string GrantType => "custom:signinwithapple_idtoken";

    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        var idToken = context.Request.Raw.Get("idtoken");
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            using var client = new HttpClient();
            var response = await client.GetFromJsonAsync<Response>("https://appleid.apple.com/auth/keys");
            var keys = response?.Keys ?? throw new ApplicationException();
            var claimsPrincipal = tokenHandler.ValidateToken(
                idToken,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = new List<string> { "https://appleid.apple.com" },
                    ValidateAudience = true,
                    ValidAudiences = new List<string> { "com.dmitriikochnev.words" },
                    ValidateLifetime = true,
                    IssuerSigningKeys = keys,
                },
                out _);

            var (user, provider, providerUserId, claims) = await FindUserFromExternalProviderAsync(claimsPrincipal);
            if (user == null)
            {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = await AutoProvisionUserAsync(provider, providerUserId, claims);
            }

            context.Result = new GrantValidationResult(user.Id, GrantType);
            return;
        }
        catch (Exception)
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
            return;
        }
    }

    private async Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)>
        FindUserFromExternalProviderAsync(ClaimsPrincipal externalUser)
    {
        // try to determine the unique id of the external user (issued by the provider)
        // the most common claim type for that are the sub claim and the NameIdentifier
        // depending on the external provider, some other claim type might be used
        var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                          externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                          throw new Exception("Unknown userid");

        // remove the user id claim so we don't include it as an extra claim if/when we provision the user
        var claims = externalUser.Claims.ToList();
        claims.Remove(userIdClaim);

        var provider = "Apple"; // TODO: review
        var providerUserId = userIdClaim.Value;

        // find external user
        var user = await _userManager.FindByLoginAsync(provider, providerUserId);

        return (user, provider, providerUserId, claims);
    }

    private async Task<ApplicationUser> AutoProvisionUserAsync(string provider, string providerUserId,
        IEnumerable<Claim> claims)
    {
        // create a list of claims that we want to transfer into our store
        var filtered = new List<Claim>();

        // user's display name
        var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                   claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        if (name != null)
        {
            filtered.Add(new Claim(JwtClaimTypes.Name, name));
        }
        else
        {
            var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                        claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
            var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                       claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
            if (first != null && last != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
            }
            else if (first != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, first));
            }
            else if (last != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, last));
            }
        }

        // email
        var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (email != null)
        {
            filtered.Add(new Claim(JwtClaimTypes.Email, email));
        }

        var user = new ApplicationUser
        {
            UserName =  Guid.NewGuid().ToString(),
        };
        var identityResult = await _userManager.CreateAsync(user);
        if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

        if (filtered.Any())
        {
            identityResult = await _userManager.AddClaimsAsync(user, filtered);
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
        }

        identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
        if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

        return user;
    }
}