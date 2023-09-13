using IdentityServer4.Stores;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Private.ConsentCallback;

[PublicAPI]
public sealed record AcceptRequest();

[PublicAPI]
public sealed record RejectRequest();

[ApiController]
[Route("api/private/consent/callback")]
public class ConsentCallbackController : ControllerBase
{
    private readonly IConsentResponseMessageStore _consentResponseMessageStore;

    public ConsentCallbackController(IConsentResponseMessageStore consentResponseMessageStore)
    {
        _consentResponseMessageStore = consentResponseMessageStore;
    }

    [HttpPost]
    [Route("accept")]
    public async Task<IActionResult> Accept([FromBody] AcceptRequest request, CancellationToken ct)
    {
        await Task.CompletedTask;
        return Problem();
    }

    [HttpPost]
    [Route("reject")]
    public async Task<IActionResult> Reject([FromBody] RejectRequest request, CancellationToken ct)
    {
        await Task.CompletedTask;
        return Problem();
    }
}