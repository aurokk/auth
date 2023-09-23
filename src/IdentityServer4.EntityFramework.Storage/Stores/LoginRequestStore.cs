using IdentityServer4.Storage.Stores;

namespace IdentityServer4.EntityFramework.Storage.Stores;

public class LoginRequestStore : ILoginRequestStore
{
    private readonly IStore _store;

    public LoginRequestStore(IStore store)
    {
        _store = store;
    }

    public async Task<LoginRequest> Get(Guid id, CancellationToken ct)
    {
        var storeItemKey = BuildKey(id);
        var storeItem = await _store.Get(storeItemKey, ct);
        var loginRequest = new LoginRequest(
            Id: id,
            Data: storeItem.Value,
            CreatedAtUtc: storeItem.CreatedAtUtc,
            RemoveAtUtc: storeItem.RemoveAtUtc
        );
        return loginRequest;
    }

    public async Task Create(LoginRequest loginRequest, CancellationToken ct)
    {
        var storeItemKey = BuildKey(loginRequest.Id);
        var storeItem = new StoreItem(
            Key: storeItemKey,
            Value: loginRequest.Data,
            CreatedAtUtc: loginRequest.CreatedAtUtc,
            RemoveAtUtc: loginRequest.RemoveAtUtc
        );
        await _store.Create(storeItem, ct);
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        var storeItemKey = BuildKey(id);
        await _store.Delete(storeItemKey, ct);
    }

    private static string BuildKey(Guid id) => $"{id:N}_loginrequest";
}