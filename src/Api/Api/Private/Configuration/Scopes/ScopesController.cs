using IdentityServer4.EntityFramework.DbContexts;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.Entities;

namespace Api.Api.Private.Configuration.Scopes;

[PublicAPI]
public record ScopeDataDto(
    string DisplayName,
    string Name
);

// @formatter:off
[PublicAPI]
public sealed record ScopeDto(
    int    Id,
    string DisplayName,
    string Name
);
// @formatter: on

[PublicAPI]
public sealed record CreateScopeRequest(
    ScopeDataDto Scope
);

[PublicAPI]
public sealed record ListScopesResponse(
    ScopeDto[] Scopes
);

// @formatter:off
[PublicAPI]
public sealed record UpdateScopeRequest(
    int          Id,
    ScopeDataDto Scope
);
// @formatter:on

[PublicAPI]
public sealed record DeleteScopeRequest(
    int Id
);

public static class Mappers
{
    public static ApiScope ToDomain(this ScopeDataDto scopeDataDto)
    {
        // @formatter:off
        return new ApiScope
        {
            DisplayName = scopeDataDto.DisplayName,
            Name        = scopeDataDto.Name,
        };
        // @formatter:on
    }

    public static ScopeDto ToDto(this ApiScope scopeDto)
    {
        // @formatter:off
        return new ScopeDto(
            DisplayName: scopeDto.DisplayName,
            Id:          scopeDto.Id,
            Name:        scopeDto.Name
        );
        // @formatter:on
    }
}

[ApiController]
[Route("api/private/configuration/scopes")]
public class ScopesController : ControllerBase
{
    private readonly ConfigurationDbContext _context;

    public ScopesController(ConfigurationDbContext context) => _context = context;

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateScope(CreateScopeRequest request, CancellationToken ct)
    {
        var dbScope = request.Scope.ToDomain();
        await _context.ApiScopes.AddAsync(dbScope, ct);
        await _context.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpGet]
    [Route("list")]
    public async Task<IActionResult> ListScopes(CancellationToken ct)
    {
        var dbScopes = await _context
            .ApiScopes
            .AsNoTracking()
            .ToListAsync(ct);
        var aScopes = dbScopes
            .Select(s => s.ToDto())
            .ToArray();
        var response = new ListScopesResponse(Scopes: aScopes);
        return Ok(response);
    }

    [HttpPut]
    [Route("update")]
    public async Task<IActionResult> UpdateScope(UpdateScopeRequest request, CancellationToken ct)
    {
        var dbScope = await _context.ApiScopes.SingleOrDefaultAsync(x => x.Id == request.Id, ct);
        if (dbScope == null)
        {
            return BadRequest();
        }

        // @formatter:off
        dbScope.DisplayName = request.Scope.DisplayName;
        dbScope.Name        = request.Scope.Name;
        // @formatter:on

        await _context.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete]
    [Route("delete")]
    public async Task<IActionResult> DeleteScopes(DeleteScopeRequest request, CancellationToken ct)
    {
        var dbScope = await _context.ApiScopes.SingleOrDefaultAsync(x => x.Id == request.Id, ct);
        if (dbScope == null)
        {
            return BadRequest();
        }

        _context.ApiScopes.Remove(dbScope);
        await _context.SaveChangesAsync(ct);
        return Ok();
    }
}