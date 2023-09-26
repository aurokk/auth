#nullable enable

using IdentityServer4.Storage.Stores;
using Newtonsoft.Json;

namespace IdentityServer4.EntityFramework.Storage.Stores;

public sealed record LoginRequestData(
    Guid AuthorizeRequestId
);

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
        var data = JsonConvert.DeserializeObject<LoginRequestData>(storeItem.Value) ??
                   throw new ApplicationException();
        var loginRequest = new LoginRequest(
            Id: id,
            AuthorizeRequestId: data.AuthorizeRequestId,
            CreatedAtUtc: storeItem.CreatedAtUtc,
            RemoveAtUtc: storeItem.RemoveAtUtc
        );
        return loginRequest;
    }

    public async Task Create(LoginRequest loginRequest, CancellationToken ct)
    {
        var storeItemKey = BuildKey(loginRequest.Id);
        var data = new LoginRequestData(
            AuthorizeRequestId: loginRequest.AuthorizeRequestId
        );
        var value = JsonConvert.SerializeObject(data);
        var storeItem = new StoreItem(
            Key: storeItemKey,
            Value: value,
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