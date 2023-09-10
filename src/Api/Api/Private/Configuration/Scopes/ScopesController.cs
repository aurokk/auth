using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Private.Configuration.Scopes;

[ApiController]
[Route("api/private/configuration/resources")]
public class ScopesController : ControllerBase
{
    // private readonly ConfigurationDbContext _context;

    [HttpPost]
    public async Task<IActionResult> CreateResource(CancellationToken ct)
    {
        await Task.CompletedTask;
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> ListResources(CancellationToken ct)
    {
        await Task.CompletedTask;
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateResource(CancellationToken ct)
    {
        await Task.CompletedTask;
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteResources(CancellationToken ct)
    {
        await Task.CompletedTask;
        return Ok();
    }
}