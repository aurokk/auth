namespace IdentityServer4.Storage.Stores;

public sealed record ConsentRequest2(
    Guid Id,
    Guid LoginRequestId,
    Guid LoginResponseId,
    DateTime CreatedAtUtc,
    DateTime RemoveAtUtc
);

public interface IConsentRequest2Store
{
    Task<ConsentRequest2> Get(Guid id, CancellationToken ct);
    Task Create(ConsentRequest2 consentRequest, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
}