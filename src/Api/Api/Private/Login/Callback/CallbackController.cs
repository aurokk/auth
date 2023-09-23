using System.Web;
using IdentityServer4.Models;
using IdentityServer4.Storage.Stores;
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
public class CallbackController : ControllerBase
{
    private readonly ILoginResponseMessageStore _loginResponseMessageStore;
    private readonly ILoginRequestIdToResponseIdMessageStore _loginRequestIdToResponseIdMessageStore;
    private readonly ILogger<CallbackController> _logger;
    private readonly ILoginRequestStore _loginRequestStore;
    private readonly ILoginResponseStore _loginResponseStore;

    public CallbackController(ILoginResponseMessageStore loginResponseMessageStore,
        ILoginRequestIdToResponseIdMessageStore loginRequestIdToResponseIdMessageStore,
        ILogger<CallbackController> logger,
        ILoginRequestStore loginRequestStore,
        ILoginResponseStore loginResponseStore)
    {
        _loginResponseMessageStore = loginResponseMessageStore;
        _loginRequestIdToResponseIdMessageStore = loginRequestIdToResponseIdMessageStore;
        _logger = logger;
        _loginRequestStore = loginRequestStore;
        _loginResponseStore = loginResponseStore;
    }

    [HttpPost]
    [Route("accept")]
    [ProducesResponseType(typeof(AcceptResponse), 200)]
    public async Task<IActionResult> Accept([FromBody] AcceptRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Request {Request}", JsonConvert.SerializeObject(request));

        if (string.IsNullOrWhiteSpace(request.LoginRequestId) ||
            !Guid.TryParse(request.LoginRequestId, out var loginRequestId)) // TODO
        {
            return BadRequest();
        }

        var subjectId = request.SubjectId;
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return BadRequest();
        }

        var loginRequest = await _loginRequestStore.Get(loginRequestId, ct); // TODO

        var loginResponse = new IdentityServer4.Storage.Stores.LoginResponse(
            Id: Guid.NewGuid(),
            LoginRequestId: loginRequestId,
            SubjectId: subjectId,
            IsSuccess: true,
            CreatedAtUtc: DateTime.UtcNow,
            RemoveAtUtc: DateTime.UtcNow + TimeSpan.FromDays(1)
        );

        await _loginResponseStore.Create(loginResponse, ct);

        // var requestIdToResponseIdMessage =
        //     await _loginRequestIdToResponseIdMessageStore.ReadAsync(request.LoginRequestId);
        //
        // var lr = new LoginResponse
        // {
        //     IsSuccess = true,
        //     SubjectId = subjectId,
        // };
        // var utcNow = DateTime.UtcNow;
        // var lrMessage = new Message<LoginResponse>(lr, utcNow);
        // await _loginResponseMessageStore.WriteAsync(requestIdToResponseIdMessage.Data.LoginResponseId, lrMessage);

        var response = new AcceptResponse(loginResponse.Id.ToString("N"));
        return Ok(response);
    }

    [HttpPost]
    [Route("reject")]
    [ProducesResponseType(typeof(RejectResponse), 200)]
    public async Task<IActionResult> Reject([FromBody] RejectRequest request, CancellationToken ct)
    {
        var id = request.LoginRequestId;
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        // var requestIdToResponseIdMessage =
        //     await _loginRequestIdToResponseIdMessageStore.ReadAsync(request.LoginRequestId);
        //
        // var lr = new LoginResponse
        // {
        //     IsSuccess = false,
        // };
        // var utcNow = DateTime.UtcNow;
        // var lrMessage = new Message<LoginResponse>(lr, utcNow);
        // await _loginResponseMessageStore.WriteAsync(requestIdToResponseIdMessage.Data.LoginResponseId, lrMessage);

        // var response = new RejectResponse(requestIdToResponseIdMessage.Data.LoginResponseId);
        // return Ok(response);

        throw new NotImplementedException();
    }
}