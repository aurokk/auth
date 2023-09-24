using IdentityServer4.Storage.Stores;
using Newtonsoft.Json;

namespace IdentityServer4.EntityFramework.Storage.Stores;

public sealed record ConsentRequestData(
    Guid AuthorizeRequestId
);

public class ConsentRequestStore : IConsentRequest2Store
{
    private readonly IStore _store;

    public ConsentRequestStore(IStore store)
    {
        _store = store;
    }

    public async Task<ConsentRequest2> Get(Guid id, CancellationToken ct)
    {
        var storeItemKey = BuildKey(id);
        var storeItem = await _store.Get(storeItemKey, ct);
        var data = JsonConvert.DeserializeObject<ConsentRequestData>(storeItem.Value) ??
                   throw new ApplicationException();
        var consentRequest = new ConsentRequest2(
            Id: id,
            AuthorizeRequestId: data.AuthorizeRequestId,
            CreatedAtUtc: storeItem.CreatedAtUtc,
            RemoveAtUtc: storeItem.RemoveAtUtc
        );
        return consentRequest;
    }

    public async Task Create(ConsentRequest2 consentRequest, CancellationToken ct)
    {
        var storeItemKey = BuildKey(consentRequest.Id);
        var data = new ConsentRequestData(
            AuthorizeRequestId: consentRequest.AuthorizeRequestId
        );
        var value = JsonConvert.SerializeObject(data);
        var storeItem = new StoreItem(
            Key: storeItemKey,
            Value: value,
            CreatedAtUtc: consentRequest.CreatedAtUtc,
            RemoveAtUtc: consentRequest.RemoveAtUtc
        );
        await _store.Create(storeItem, ct);
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        var storeItemKey = BuildKey(id);
        await _store.Delete(storeItemKey, ct);
    }

    private static string BuildKey(Guid id) => $"{id:N}_consentrequest";
}