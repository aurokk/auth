using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Api.Private.Configuration.Clients.Secrets;

// @formatter:off
[PublicAPI]
public sealed record SecretDto(
    int       Id,
    string    Description,
    DateTime  Created,
    DateTime? Expiration
);

[PublicAPI]
public sealed record CreateSecretDataDto(
    string    Description,
    DateTime? Expiration,
    string    Value
);

[PublicAPI]
public sealed record UpdateSecretDataDto(
    string    Description,
    DateTime? Expiration,
    string?   Value
);
// @formatter: on

[PublicAPI]
public sealed record CreateSecretRequest(
    CreateSecretDataDto Secret
);

[PublicAPI]
public sealed record UpdateSecretRequest(
    UpdateSecretDataDto Secret
);

[PublicAPI]
public sealed record ListSecretsResponse(
    SecretDto[] Secrets
);

public static class Mappers
{
    // public static Client ToDomain(this ClientDataDto clientDataDto)
    // {
    //     // @formatter:off
    //     return new Client
    //     {
    //         AbsoluteRefreshTokenLifetime = clientDataDto.AbsoluteRefreshTokenLifetime,
    //         AccessTokenLifetime = clientDataDto.AccessTokenLifetime,
    //         AllowedGrantTypes = clientDataDto.AllowedGrantTypes.Select(x => new ClientGrantType {GrantType = x}).ToList(),
    //         AllowedScopes = clientDataDto.AllowedScopes.Select(x => new ClientScope {Scope = x}).ToList(),
    //         AllowAccessTokensViaBrowser = clientDataDto.AllowAccessTokensViaBrowser,
    //         AllowOfflineAccess = clientDataDto.AllowOfflineAccess,
    //         ClientId = clientDataDto.ClientId,
    //         RedirectUris = clientDataDto.RedirectUris.Select(x => new ClientRedirectUri {RedirectUri = x}).ToList(),
    //         RefreshTokenExpiration = clientDataDto.RefreshTokenExpiration,
    //         RefreshTokenUsage = clientDataDto.RefreshTokenUsage,
    //         RequireClientSecret = clientDataDto.RequireClientSecret,
    //         RequirePkce = clientDataDto.RequirePkce,
    //         SlidingRefreshTokenLifetime = clientDataDto.SlidingRefreshTokenLifetime
    //     };
    //     // @formatter:on
    // }

    public static SecretDto ToDto(this ClientSecret secret)
    {
        // @formatter:off
        return new SecretDto(
            Id:          secret.Id,
            Description: secret.Description,
            Created:     secret.Created,
            Expiration:  secret.Expiration
        );
        // @formatter:on
    }
}

[ApiController]
[Route("api/private/configuration/clients/{clientId}/secrets")]
public class SecretsController : ControllerBase
{
    private readonly ConfigurationDbContext _context;

    public SecretsController(ConfigurationDbContext context) => _context = context;

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> CreateSecret(
        [FromRoute] int clientId,
        [FromBody] CreateSecretRequest request,
        CancellationToken ct)
    {
        var dbClient = await _context
            .Clients
            .Include(x => x.ClientSecrets)
            .SingleOrDefaultAsync(x => x.Id == clientId, ct);

        if (dbClient == null)
        {
            return BadRequest();
        }

        var aSecret = new ClientSecret
        {
            Description = request.Secret.Description,
            Expiration = request.Secret.Expiration,
            Value = request.Secret.Value.ToSha256(),
        };
        dbClient.ClientSecrets.Add(aSecret);
        await _context.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> ListSecrets(
        [FromRoute] int clientId,
        CancellationToken ct)
    {
        var dbClient = await _context.Clients
            .Include(x => x.ClientSecrets)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == clientId, ct);

        if (dbClient == null)
        {
            return BadRequest();
        }

        var dbSecrets = dbClient.ClientSecrets;
        var aSecrets = dbSecrets.Select(s => s.ToDto()).ToArray();
        var response = new ListSecretsResponse(Secrets: aSecrets);
        return Ok(response);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> UpdateSecret(
        [FromRoute] int clientId,
        [FromRoute] int id,
        [FromBody] UpdateSecretRequest request,
        CancellationToken ct)
    {
        var dbClient = await _context
            .Clients
            .Include(x => x.ClientSecrets)
            .SingleOrDefaultAsync(x => x.Id == clientId, ct);

        if (dbClient == null)
        {
            return BadRequest();
        }

        var dbSecret = dbClient
            .ClientSecrets
            .SingleOrDefault(x => x.Id == id);

        if (dbSecret == null)
        {
            return BadRequest();
        }

        dbSecret.Description = request.Secret.Description;
        dbSecret.Expiration = request.Secret.Expiration;
        if (request.Secret.Value != null)
        {
            dbSecret.Value = request.Secret.Value.ToSha256();
        }

        await _context.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete]
    [Route("")]
    public async Task<IActionResult> DeleteSecret(
        [FromRoute] int clientId,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var dbClient = await _context
            .Clients
            .Include(x => x.ClientSecrets)
            .SingleOrDefaultAsync(x => x.Id == clientId, ct);

        if (dbClient == null)
        {
            return BadRequest();
        }

        var dbSecret = dbClient
            .ClientSecrets
            .SingleOrDefault(x => x.Id == id);

        if (dbSecret == null)
        {
            return BadRequest();
        }

        dbClient.ClientSecrets.Remove(dbSecret);
        await _context.SaveChangesAsync(ct);
        return Ok();
    }
}