#nullable enable

using IdentityServer4.Storage.Stores;

namespace IdentityServer4.EntityFramework.Storage.Stores;

// authorizeRequest
// <- loginRequest (opt)
//    <- loginResponse
// <- consentRequest (opt)
//    <- consentResponse

public class AuthorizeRequest2Store : IAuthorizeRequest2Store
{
    private readonly IStore _store;

    public AuthorizeRequest2Store(IStore store)
    {
        _store = store;
    }

    public async Task<AuthorizeRequest2?> TryGet(Guid id, CancellationToken ct)
    {
        var storeItemKey = BuildKey(id);
        var storeItem = await _store.Get(storeItemKey, ct);
        var authorizeRequest = new AuthorizeRequest2(
            Id: id,
            Data: storeItem.Value,
            CreatedAtUtc: storeItem.CreatedAtUtc,
            RemoveAtUtc: storeItem.RemoveAtUtc
        );
        return authorizeRequest;
    }

    public async Task<AuthorizeRequest2> Get(Guid id, CancellationToken ct)
    {
        var storeItemKey = BuildKey(id);
        var storeItem = await _store.Get(storeItemKey, ct);
        var authorizeRequest = new AuthorizeRequest2(
            Id: id,
            Data: storeItem.Value,
            CreatedAtUtc: storeItem.CreatedAtUtc,
            RemoveAtUtc: storeItem.RemoveAtUtc
        );
        return authorizeRequest;
    }

    public async Task Create(AuthorizeRequest2 authorizeRequest, CancellationToken ct)
    {
        var storeItemKey = BuildKey(authorizeRequest.Id);
        var storeItem = new StoreItem(
            Key: storeItemKey,
            Value: authorizeRequest.Data,
            CreatedAtUtc: authorizeRequest.CreatedAtUtc,
            RemoveAtUtc: authorizeRequest.RemoveAtUtc
        );
        await _store.Create(storeItem, ct);
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        var storeItemKey = BuildKey(id);
        await _store.Delete(storeItemKey, ct);
    }

    private static string BuildKey(Guid id) => $"{id:N}_authorizerequest";
}