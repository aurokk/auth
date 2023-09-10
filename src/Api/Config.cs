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
            new ApiScope("protected-resource.read", "Read access to Protected Resource"),
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("protected-resource", "Protected Resource")
            {
                Scopes = { "protected-resource.read", },
            },
        };

    public static IEnumerable<Client> Clients =>
        new[]
        {
            new Client
            {
                AccessTokenLifetime = 1 * 24 * 60 * 60, // 1 day
                AllowedGrantTypes = { GrantType.Implicit, },
                AllowedScopes = { "protected-resource.read", },
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
                AllowedScopes = { "protected-resource.read", },
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