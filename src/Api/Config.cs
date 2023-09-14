using IdentityServer4.Models;

namespace Api;

public static class Config
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope
            {
                Description = "Description",
                DisplayName = "Read access to Protected Resource",
                Name = "beam.read",
            },
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource
            {
                Description = "Description",
                DisplayName = "Protected Resource",
                Name = "beam",
                Scopes = { "beam.read", },
            },
        };

    public static Client[] Clients =>
        new[]
        {
            new Client
            {
                AccessTokenLifetime = 1 * 24 * 60 * 60, // 1 day
                AllowedGrantTypes = { GrantType.Implicit, },
                AllowedScopes = { "beam.read", },
                AllowAccessTokensViaBrowser = true,
                AllowOfflineAccess = false,
                AllowRememberConsent = true,
                ClientId = "implicit",
                ConsentLifetime = null,
                RedirectUris = { "http://localhost:10000", },
                RequireConsent = true,
            },
            new Client
            {
                AbsoluteRefreshTokenLifetime = 0,
                AccessTokenLifetime = 1 * 24 * 60 * 60, // 1 day
                AllowedGrantTypes = { GrantType.AuthorizationCode, },
                AllowedScopes = { "beam.read", },
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
}