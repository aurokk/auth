#nullable enable

namespace IdentityServer4.Storage.Stores;

public sealed record AuthorizeRequest2(
    Guid Id,
    string Data,
    DateTime CreatedAtUtc,
    DateTime RemoveAtUtc
);

public interface IAuthorizeRequest2Store
{
    Task<AuthorizeRequest2?> TryGet(Guid id, CancellationToken ct);
    Task<AuthorizeRequest2> Get(Guid id, CancellationToken ct);
    Task Create(AuthorizeRequest2 authorizeRequest, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
}