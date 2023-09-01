using System.Security.Claims;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Api;

public static class Config
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope(name: "words-api"),
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("words-api", "Words Api")
            {
                Scopes = { "words-api", },
            },
        };

    public static IEnumerable<Client> Clients =>
        new[]
        {
            new Client
            {
                AccessTokenLifetime = 1 * 24 * 60 * 60, // 1 day
                AllowedGrantTypes = { GrantType.Implicit, },
                AllowedScopes = { "words-api", },
                AllowAccessTokensViaBrowser = true,
                AllowOfflineAccess = false,
                ClientId = "implicit",
                RedirectUris = { "http://localhost:10000", },
            },
            new Client
            {
                AbsoluteRefreshTokenLifetime = 0,
                AccessTokenLifetime = 1 * 24 * 60 * 60, // 1 day
                AllowedGrantTypes = { GrantType.AuthorizationCode, },
                AllowedScopes = { "words-api", },
                AllowAccessTokensViaBrowser = false,
                AllowOfflineAccess = true,
                ClientId = "authorization-code",
                ClientSecrets = { new Secret("authorization-code-secret".Sha256()) },
                RedirectUris = { "http://localhost:10010", },
                RefreshTokenExpiration = TokenExpiration.Sliding,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RequireClientSecret = true,
                RequirePkce = false,
                SlidingRefreshTokenLifetime = 14 * 24 * 60 * 60, // 14 days
            },
        };

    public static List<TestUser> Users => new()
    {
        new TestUser
        {
            SubjectId = "818727",
            Username = "alice",
            Password = "alice",
            Claims =
            {
                new Claim(JwtClaimTypes.Name, "Alice Smith"),
                new Claim(JwtClaimTypes.GivenName, "Alice"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                new Claim(JwtClaimTypes.Address,
                    @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }",
                    IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
            },
        },
        new TestUser
        {
            SubjectId = "88421113",
            Username = "bob",
            Password = "bob",
            Claims =
            {
                new Claim(JwtClaimTypes.Name, "Bob Smith"),
                new Claim(JwtClaimTypes.GivenName, "Bob"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                new Claim(JwtClaimTypes.Address,
                    @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }",
                    IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
                new Claim("location", "somewhere"),
            },
        },
    };
}