namespace IdentityServer4.Storage.Stores;

public sealed record LoginRequest(
    Guid Id,
    Guid AuthorizeRequestId,
    DateTime CreatedAtUtc,
    DateTime RemoveAtUtc
);

public interface ILoginRequestStore
{
    Task<LoginRequest> Get(Guid id, CancellationToken ct);
    Task Create(LoginRequest loginRequest, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
}