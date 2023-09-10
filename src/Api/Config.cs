using IdentityServer4.Models;

namespace Api;

public static class Config
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("protected-resource.read", "Read access to Protected Resource")
            {
                Description = "Description",
            },
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("protected-resource", "Protected Resource")
            {
                AllowedAccessTokenSigningAlgorithms = new[]
                {
                    "RS256",
                    "RS384",
                    "RS512",
                    "PS256",
                    "PS384",
                    "PS512",
                    "ES256",
                    "ES384",
                    "ES512",
                },
                Scopes = { "protected-resource.read", },
                Description = "Description",
            },
        };

    public static Client[] Clients =>
        new[]
        {
            new Client
            {
                AccessTokenLifetime = 1 * 24 * 60 * 60, // 1 day
                AllowedGrantTypes = { GrantType.Implicit, },
                AllowedIdentityTokenSigningAlgorithms = new[]
                {
                    "RS256",
                    "RS384",
                    "RS512",
                    "PS256",
                    "PS384",
                    "PS512",
                    "ES256",
                    "ES384",
                    "ES512",
                },
                AllowedScopes = { "protected-resource.read", },
                AllowAccessTokensViaBrowser = true,
                AllowOfflineAccess = false,
                BackChannelLogoutUri = "http://implicit",
                ClientId = "implicit",
                ClientName = "Implicit",
                ClientUri = "http://implicit",
                Description = "Description",
                FrontChannelLogoutUri = "http://implicit",
                LogoUri = "http://implicit",
                RedirectUris = { "http://localhost:10000", },
            },
            new Client
            {
                AbsoluteRefreshTokenLifetime = 0,
                AccessTokenLifetime = 1 * 24 * 60 * 60, // 1 day
                AllowedGrantTypes = { GrantType.AuthorizationCode, },
                AllowedIdentityTokenSigningAlgorithms = new[]
                {
                    "RS256",
                    "RS384",
                    "RS512",
                    "PS256",
                    "PS384",
                    "PS512",
                    "ES256",
                    "ES384",
                    "ES512",
                },
                AllowedScopes =
                {
                    "protected-resource.read",
                },
                AllowAccessTokensViaBrowser = false,
                AllowOfflineAccess = true,
                BackChannelLogoutUri = "http://authorizationcode",
                ClientId = "authorization-code",
                ClientName = "AuthorizationCode",
                ClientUri = "http://authorizationcode",
                ClientSecrets = { new Secret("authorization-code-secret".Sha256()) },
                Description = "Description",
                FrontChannelLogoutUri = "http://authorizationcode",
                LogoUri = "http://authorizationcode",
                RedirectUris = { "http://localhost:10010", },
                RefreshTokenExpiration = TokenExpiration.Sliding,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RequireClientSecret = true,
                RequirePkce = false,
                SlidingRefreshTokenLifetime = 14 * 24 * 60 * 60, // 14 days
            },
        };
}