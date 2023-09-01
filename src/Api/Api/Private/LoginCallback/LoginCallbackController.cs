using IdentityServer4.Models;
using IdentityServer4.Stores;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Private.LoginCallback;

[PublicAPI]
public sealed record AcceptRequest(
    string? LoginRequestId,
    string? SubjectId
);

[PublicAPI]
public sealed record RejectRequest(
    string? LoginRequestId
);

[ApiController]
[Route("api/private/login/callback")]
public class LoginCallbackController : ControllerBase
{
    private readonly ILoginResponseMessageStore _loginResponseMessageStore;
    private readonly ILoginRequestIdToResponseIdMessageStore _loginRequestIdToResponseIdMessageStore;

    public LoginCallbackController(ILoginResponseMessageStore loginResponseMessageStore,
        ILoginRequestIdToResponseIdMessageStore loginRequestIdToResponseIdMessageStore)
    {
        _loginResponseMessageStore = loginResponseMessageStore;
        _loginRequestIdToResponseIdMessageStore = loginRequestIdToResponseIdMessageStore;
    }

    [HttpPost]
    [Route("accept")]
    public async Task<IActionResult> Accept(AcceptRequest request, CancellationToken ct)
    {
        var id = request.LoginRequestId;
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var subjectId = request.SubjectId;
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return BadRequest();
        }

        var requestIdToResponseIdMessage =
            await _loginRequestIdToResponseIdMessageStore.ReadAsync(request.LoginRequestId);

        var lr = new LoginResponse
        {
            IsSuccess = true,
            SubjectId = subjectId,
        };
        var utcNow = DateTime.UtcNow;
        var lrMessage = new Message<LoginResponse>(lr, utcNow);
        await _loginResponseMessageStore.WriteAsync(requestIdToResponseIdMessage.Data.LoginResponseId, lrMessage);

        return Ok(new { LoginResponseId = requestIdToResponseIdMessage.Data.LoginResponseId, });
    }

    [HttpPost]
    [Route("reject")]
    public async Task<IActionResult> Reject(RejectRequest request, CancellationToken ct)
    {
        var id = request.LoginRequestId;
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var requestIdToResponseIdMessage =
            await _loginRequestIdToResponseIdMessageStore.ReadAsync(request.LoginRequestId);

        var lr = new LoginResponse
        {
            IsSuccess = false,
        };
        var utcNow = DateTime.UtcNow;
        var lrMessage = new Message<LoginResponse>(lr, utcNow);
        await _loginResponseMessageStore.WriteAsync(requestIdToResponseIdMessage.Data.LoginResponseId, lrMessage);

        return Ok(new { LoginResponseId = requestIdToResponseIdMessage.Data.LoginResponseId, });
    }
}