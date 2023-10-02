using IdentityServer4.Storage.Stores;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Api.Api.Private.Login.Callback;

[PublicAPI]
public sealed record AcceptRequest(
    string? LoginRequestId,
    string? SubjectId
);

[PublicAPI]
public sealed record AcceptResponse(
    string? LoginResponseId
);

[ApiExplorerSettings(GroupName = SwaggerPrivateExtensions.Name)]
[ApiController]
[Route("api/private/login/callback")]
public class CallbackController : ControllerBase
{
    private readonly ILogger<CallbackController> _logger;
    private readonly ILoginRequestStore _loginRequestStore;
    private readonly ILoginResponseStore _loginResponseStore;

    public CallbackController(
        ILogger<CallbackController> logger,
        ILoginRequestStore loginRequestStore,
        ILoginResponseStore loginResponseStore)
    {
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
}