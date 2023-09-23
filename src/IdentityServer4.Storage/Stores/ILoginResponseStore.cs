# nullable enable

namespace IdentityServer4.Storage.Stores;

public sealed record LoginResponse(
    Guid Id,
    Guid LoginRequestId,
    bool IsSuccess,
    string? SubjectId,
    DateTime CreatedAtUtc,
    DateTime RemoveAtUtc
);

public interface ILoginResponseStore
{
    Task<LoginResponse> Get(Guid id, CancellationToken ct);
    Task Create(LoginResponse loginResponse, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
}