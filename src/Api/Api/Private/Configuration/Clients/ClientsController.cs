using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Api.Private.Configuration.Clients;

// @formatter:off
[PublicAPI]
public record ClientDataDto(
    int      AbsoluteRefreshTokenLifetime,
    int      AccessTokenLifetime,
    string[] AllowedGrantTypes,
    string[] AllowedScopes,
    bool     AllowAccessTokensViaBrowser,
    bool     AllowOfflineAccess,
    string   ClientId,
    string[] RedirectUris,
    int      RefreshTokenExpiration,
    int      RefreshTokenUsage,
    bool     RequireClientSecret,
    bool     RequirePkce,
    int      SlidingRefreshTokenLifetime
);
// @formatter:on

// @formatter:off
[PublicAPI]
public sealed record ClientDto(
    int      AbsoluteRefreshTokenLifetime,
    int      AccessTokenLifetime,
    string[] AllowedGrantTypes,
    string[] AllowedScopes,
    bool     AllowAccessTokensViaBrowser,
    bool     AllowOfflineAccess,
    string   ClientId,
    int[]    ClientSecrets,
    int      Id,
    string[] RedirectUris,
    int      RefreshTokenExpiration,
    int      RefreshTokenUsage,
    bool     RequireClientSecret,
    bool     RequirePkce,
    int      SlidingRefreshTokenLifetime
);
// @formatter: on

[PublicAPI]
public sealed record CreateClientRequest(
    ClientDataDto Client
);

[PublicAPI]
public sealed record ListClientsResponse(
    ClientDto[] Clients
);

// @formatter:off
[PublicAPI]
public sealed record UpdateClientRequest(
    int           Id,
    ClientDataDto Client
);
// @formatter:on

[PublicAPI]
public sealed record DeleteClientRequest(
    int Id
);

public static class Mappers
{
    public static Client ToDomain(this ClientDataDto clientDataDto)
    {
        // @formatter:off
        return new Client
        {
            AbsoluteRefreshTokenLifetime = clientDataDto.AbsoluteRefreshTokenLifetime,
            AccessTokenLifetime = clientDataDto.AccessTokenLifetime,
            AllowedGrantTypes = clientDataDto.AllowedGrantTypes.Select(x => new ClientGrantType {GrantType = x}).ToList(),
            AllowedScopes = clientDataDto.AllowedScopes.Select(x => new ClientScope {Scope = x}).ToList(),
            AllowAccessTokensViaBrowser = clientDataDto.AllowAccessTokensViaBrowser,
            AllowOfflineAccess = clientDataDto.AllowOfflineAccess,
            ClientId = clientDataDto.ClientId,
            RedirectUris = clientDataDto.RedirectUris.Select(x => new ClientRedirectUri {RedirectUri = x}).ToList(),
            RefreshTokenExpiration = clientDataDto.RefreshTokenExpiration,
            RefreshTokenUsage = clientDataDto.RefreshTokenUsage,
            RequireClientSecret = clientDataDto.RequireClientSecret,
            RequirePkce = clientDataDto.RequirePkce,
            SlidingRefreshTokenLifetime = clientDataDto.SlidingRefreshTokenLifetime
        };
        // @formatter:on
    }

    public static ClientDto ToDto(this Client client)
    {
        // @formatter:off
        return new ClientDto(
            AbsoluteRefreshTokenLifetime: client.AbsoluteRefreshTokenLifetime,
            AccessTokenLifetime: client.AccessTokenLifetime,
            AllowedGrantTypes: client.AllowedGrantTypes.Select(x => x.GrantType).ToArray(),
            AllowedScopes: client.AllowedScopes.Select(x => x.Scope).ToArray(),
            AllowAccessTokensViaBrowser: client.AllowAccessTokensViaBrowser,
            AllowOfflineAccess: client.AllowOfflineAccess,
            ClientId: client.ClientId,
            ClientSecrets: client.ClientSecrets.Select(x => x.Id).ToArray(),
            Id: client.Id,
            RedirectUris: client.RedirectUris.Select(x => x.RedirectUri).ToArray(),
            RefreshTokenExpiration: client.RefreshTokenExpiration,
            RefreshTokenUsage: client.RefreshTokenUsage,
            RequireClientSecret: client.RequireClientSecret,
            RequirePkce: client.RequirePkce,
            SlidingRefreshTokenLifetime: client.SlidingRefreshTokenLifetime
        );
        // @formatter:on
    }
}

[ApiController]
[Route("api/private/configuration/clients")]
public class ClientsController : ControllerBase
{
    private readonly ConfigurationDbContext _context;

    public ClientsController(ConfigurationDbContext context) => _context = context;

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateClient(CreateClientRequest request, CancellationToken ct)
    {
        var dbClient = request.Client.ToDomain();
        await _context.Clients.AddAsync(dbClient, ct);
        await _context.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpGet]
    [Route("list")]
    public async Task<IActionResult> ListClients(CancellationToken ct)
    {
        var dbClients = await _context
            .Clients
            .AsNoTracking()
            .ToListAsync(ct);
        var aClients = dbClients
            .Select(s => s.ToDto())
            .ToArray();
        var response = new ListClientsResponse(Clients: aClients);
        return Ok(response);
    }

    [HttpPut]
    [Route("update")]
    public async Task<IActionResult> UpdateClient(UpdateClientRequest request, CancellationToken ct)
    {
        var dbClient = await _context
            .Clients
            .Include(client => client.AllowedGrantTypes)
            .Include(client => client.AllowedScopes)
            .Include(client => client.RedirectUris)
            .SingleOrDefaultAsync(x => x.Id == request.Id, ct);
        if (dbClient == null)
        {
            return BadRequest();
        }

        // @formatter:off
        dbClient.AbsoluteRefreshTokenLifetime = request.Client.AbsoluteRefreshTokenLifetime;
        dbClient.AccessTokenLifetime = request.Client.AccessTokenLifetime;
        //
        dbClient.AllowedGrantTypes.RemoveAll(s => !request.Client.AllowedGrantTypes.Contains(s.GrantType));
        var currAllowedGrantTypes = dbClient.AllowedGrantTypes.Select(x => x.GrantType).ToHashSet();
        var reqAllowedGrantTypes = request.Client.AllowedGrantTypes.ToHashSet();
        var newAllowedGrantTypes = reqAllowedGrantTypes.Except(currAllowedGrantTypes).Select(grantType => new ClientGrantType {GrantType = grantType,});
        dbClient.AllowedGrantTypes.AddRange(newAllowedGrantTypes);
        //
        dbClient.AllowedScopes.RemoveAll(s => !request.Client.AllowedScopes.Contains(s.Scope));
        var currAllowedScopes = dbClient.AllowedScopes.Select(x => x.Scope).ToHashSet();
        var reqAllowedScopes = request.Client.AllowedScopes.ToHashSet();
        var newAllowedScopes = reqAllowedScopes.Except(currAllowedScopes).Select(scope => new ClientScope {Scope = scope,});
        dbClient.AllowedScopes.AddRange(newAllowedScopes);
        //
        dbClient.AllowAccessTokensViaBrowser = request.Client.AllowAccessTokensViaBrowser;
        dbClient.AllowOfflineAccess = request.Client.AllowOfflineAccess;
        dbClient.ClientId = request.Client.ClientId;
        //
        dbClient.RedirectUris.RemoveAll(s => !request.Client.RedirectUris.Contains(s.RedirectUri));
        var currRedirectUris = dbClient.RedirectUris.Select(x => x.RedirectUri).ToHashSet();
        var reqRedirectUris = request.Client.RedirectUris.ToHashSet();
        var newRedirectUris = reqRedirectUris.Except(currRedirectUris).Select(redirectUrl => new ClientRedirectUri() {RedirectUri = redirectUrl,});
        dbClient.RedirectUris.AddRange(newRedirectUris);
        //
        dbClient.RefreshTokenExpiration = request.Client.RefreshTokenExpiration;
        dbClient.RefreshTokenUsage = request.Client.RefreshTokenUsage;
        dbClient.RequireClientSecret = request.Client.RequireClientSecret;
        dbClient.RequirePkce = request.Client.RequirePkce;
        dbClient.SlidingRefreshTokenLifetime = request.Client.SlidingRefreshTokenLifetime;
        // @formatter:on

        await _context.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete]
    [Route("delete")]
    public async Task<IActionResult> DeleteClients(DeleteClientRequest request, CancellationToken ct)
    {
        var dbClient = await _context.Clients.SingleOrDefaultAsync(x => x.Id == request.Id, ct);
        if (dbClient == null)
        {
            return BadRequest();
        }

        _context.Clients.Remove(dbClient);
        await _context.SaveChangesAsync(ct);
        return Ok();
    }
}