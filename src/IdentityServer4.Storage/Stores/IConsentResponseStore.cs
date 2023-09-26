namespace IdentityServer4.Storage.Stores;

public sealed record ConsentResponse2(
    Guid Id,
    Guid ConsentRequestId,
    DateTime CreatedAtUtc,
    DateTime RemoveAtUtc
);

public interface IConsentResponse2Store
{
    Task<ConsentResponse2> Get(Guid id, CancellationToken ct);
    Task Create(ConsentResponse2 consentResponse, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
}