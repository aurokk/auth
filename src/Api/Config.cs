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
            // web app
            new Client
            {
                ClientId = "web-app",
                AllowedGrantTypes =
                {
                    GrantType.AuthorizationCode,
                    // GrantType.ResourceOwnerPassword,
                    // "custom:signinwithapple_idtoken"
                    
                },
                AllowedScopes = { "words-api", },
                RequireClientSecret = false,
                RequirePkce = false,
                AllowOfflineAccess = false,
                AccessTokenLifetime = 1 * 24 * 60 * 60, // 1 day
                RedirectUris =
                {
                    "http://localhost:10000",
                    // "vocabu://auth-callback",
                },
            },

            // mobile app
            // new Client
            // {
            //     ClientId = "words-app",
            //     AllowedGrantTypes =
            //     {
            //         GrantType.AuthorizationCode,
            //         GrantType.ResourceOwnerPassword,
            //         "custom:signinwithapple_idtoken"
            //     },
            //     RequireClientSecret = false,
            //     AllowedScopes = { "words-api", },
            //     RequirePkce = true,
            //     AllowOfflineAccess = true,
            //     RefreshTokenUsage = TokenUsage.OneTimeOnly,
            //     RefreshTokenExpiration = TokenExpiration.Sliding,
            //     AbsoluteRefreshTokenLifetime = 0,
            //     AccessTokenLifetime = 7 * 24 * 60 * 60,
            //     SlidingRefreshTokenLifetime = 365 * 24 * 60 * 60,
            //     // AccessTokenLifetime = 15,
            //     // SlidingRefreshTokenLifetime = 60,
            //     RedirectUris =
            //     {
            //         "vocabu://auth-callback",
            //     },
            // },
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