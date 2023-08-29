using IdentityServer4.Models;

namespace Api;

public static class Config
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new[]
        {
            new ApiScope(name: "words-api"),
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new[]
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
                ClientId = "words-app",
                AllowedGrantTypes =
                {
                    GrantType.AuthorizationCode,
                    GrantType.ResourceOwnerPassword,
                    "custom:signinwithapple_idtoken"
                },
                RequireClientSecret = false,
                AllowedScopes = { "words-api", },
                RequirePkce = true,
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                AbsoluteRefreshTokenLifetime = 0,
                AccessTokenLifetime = 7 * 24 * 60 * 60,
                SlidingRefreshTokenLifetime = 365 * 24 * 60 * 60,
                // AccessTokenLifetime = 15,
                // SlidingRefreshTokenLifetime = 60,
                RedirectUris =
                {
                    "vocabu://auth-callback",
                },
            }
        };
}