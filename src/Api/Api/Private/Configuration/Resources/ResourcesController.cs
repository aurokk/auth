using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Api.Private.Configuration.Resources;

// @formatter:off
[PublicAPI]
public record ResourceDataDto(
    string   DisplayName,
    string   Name,
    string[] Scopes
);

[PublicAPI]
public sealed record ResourceDto(
    int      Id,
    string   DisplayName,
    string   Name,
    string[] Scopes
);
// @formatter: on

[PublicAPI]
public sealed record CreateResourceRequest(
    ResourceDataDto Resource
);

[PublicAPI]
public sealed record ListResourcesResponse(
    ResourceDto[] Resources
);

[PublicAPI]
public sealed record UpdateResourceRequest(
    ResourceDataDto Resource
);

public static class Mappers
{
    public static ApiResource ToDomain(this ResourceDataDto resourceDataDto)
    {
        // @formatter:off
        return new ApiResource
        {
            DisplayName = resourceDataDto.DisplayName,
            Name        = resourceDataDto.Name,
            Scopes      = resourceDataDto.Scopes
                .Select(scope => new ApiResourceScope {Scope = scope,})
                .ToList(),
        };
        // @formatter:on
    }

    public static ResourceDto ToDto(this ApiResource resourceDto)
    {
        // @formatter:off
        return new ResourceDto(
            DisplayName: resourceDto.DisplayName,
            Id:          resourceDto.Id,
            Name:        resourceDto.Name,
            Scopes:      resourceDto.Scopes
                .Select(s => s.Scope)
                .ToArray()
        );
        // @formatter:on
    }
}

[ApiController]
[Route("api/private/configuration/resources")]
public class ResourcesController : ControllerBase
{
    private readonly ConfigurationDbContext _context;

    public ResourcesController(ConfigurationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> CreateResource(
        [FromBody] CreateResourceRequest request,
        CancellationToken ct)
    {
        var dbResource = request.Resource.ToDomain();
        await _context.ApiResources.AddAsync(dbResource, ct);
        await _context.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> ListResources(
        CancellationToken ct)
    {
        var dbResources = await _context
            .ApiResources
            .Include(r => r.Scopes)
            .AsNoTracking()
            .ToListAsync(ct);
        var aResources = dbResources
            .Select(s => s.ToDto())
            .ToArray();
        var response = new ListResourcesResponse(Resources: aResources);
        return Ok(response);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> UpdateResource(
        [FromRoute] int id,
        [FromBody] UpdateResourceRequest request,
        CancellationToken ct)
    {
        var dbResource = await _context
            .ApiResources
            .Include(r => r.Scopes)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (dbResource == null)
        {
            return BadRequest();
        }

        // @formatter:off
        dbResource.DisplayName = request.Resource.DisplayName;
        dbResource.Name        = request.Resource.Name;
        dbResource.Scopes.RemoveAll(s => !request.Resource.Scopes.Contains(s.Scope));
        var currScopes = dbResource.Scopes.Select(x => x.Scope).ToHashSet();
        var reqScopes = request.Resource.Scopes.ToHashSet();
        var newScopes = reqScopes.Except(currScopes).Select(scope => new ApiResourceScope {Scope = scope,});
        dbResource.Scopes.AddRange(newScopes);
        // @formatter:on

        await _context.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteResource(
        [FromRoute] int id,
        CancellationToken ct)
    {
        var dbResource = await _context
            .ApiResources
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (dbResource == null)
        {
            return BadRequest();
        }

        _context.ApiResources.Remove(dbResource);
        await _context.SaveChangesAsync(ct);
        return Ok();
    }
}