namespace IdentityServer4.Storage.Stores;

public sealed record StoreItem(
    string Key,
    string Value,
    DateTime CreatedAtUtc,
    DateTime RemoveAtUtc
);

public interface IStore
{
    Task<StoreItem> Get(string key, CancellationToken ct);
    Task Create(StoreItem item, CancellationToken ct);
    Task Delete(string key, CancellationToken ct);
}