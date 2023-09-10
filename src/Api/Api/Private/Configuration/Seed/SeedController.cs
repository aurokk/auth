using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Private.Configuration.Seed;

[ApiController]
[Route("api/private/configuration/seed")]
public class SeedController : ControllerBase
{
    // Контроллер только для разработки
    // Будет удален, когда соответствующие апи будут заимплеменчены
    private readonly ConfigurationDbContext _configurationDbContext;

    public SeedController(ConfigurationDbContext configurationDbContext)
    {
        _configurationDbContext = configurationDbContext;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Do(CancellationToken ct)
    {
        await _configurationDbContext.ApiResources.AddRangeAsync(Config.ApiResources.Select(r => r.ToEntity()), ct);
        await _configurationDbContext.ApiScopes.AddRangeAsync(Config.ApiScopes.Select(s => s.ToEntity()), ct);
        await _configurationDbContext.Clients.AddRangeAsync(Config.Clients.Select(c => c.ToEntity()), ct);
        await _configurationDbContext.SaveChangesAsync(ct);
        return Ok();
    }
}