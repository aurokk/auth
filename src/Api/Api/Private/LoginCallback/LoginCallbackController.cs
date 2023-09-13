using IdentityServer4.Models;
using IdentityServer4.Stores;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Api.Api.Private.LoginCallback;

[PublicAPI]
public sealed record AcceptRequest(
    string? LoginRequestId,
    string? SubjectId
);

[PublicAPI]
public sealed record AcceptResponse(
    string? LoginResponseId
);

[PublicAPI]
public sealed record RejectRequest(
    string? LoginRequestId
);

[PublicAPI]
public sealed record RejectResponse(
    string? LoginResponseId
);

[ApiExplorerSettings(GroupName = SwaggerPrivateExtensions.Name)]
[ApiController]
[Route("api/private/login/callback")]
public class LoginCallbackController : ControllerBase
{
    private readonly ILoginResponseMessageStore _loginResponseMessageStore;
    private readonly ILoginRequestIdToResponseIdMessageStore _loginRequestIdToResponseIdMessageStore;
    private readonly ILogger<LoginCallbackController> _logger;

    public LoginCallbackController(ILoginResponseMessageStore loginResponseMessageStore,
        ILoginRequestIdToResponseIdMessageStore loginRequestIdToResponseIdMessageStore,
        ILogger<LoginCallbackController> logger)
    {
        _loginResponseMessageStore = loginResponseMessageStore;
        _loginRequestIdToResponseIdMessageStore = loginRequestIdToResponseIdMessageStore;
        _logger = logger;
    }

    [HttpPost]
    [Route("accept")]
    [ProducesResponseType(typeof(AcceptResponse), 200)]
    public async Task<IActionResult> Accept(AcceptRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Request {Request}", JsonConvert.SerializeObject(request));

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

        var response = new AcceptResponse(requestIdToResponseIdMessage.Data.LoginResponseId);
        return Ok(response);
    }

    [HttpPost]
    [Route("reject")]
    [ProducesResponseType(typeof(RejectResponse), 200)]
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

        var response = new RejectResponse(requestIdToResponseIdMessage.Data.LoginResponseId);
        return Ok(response);
    }
}