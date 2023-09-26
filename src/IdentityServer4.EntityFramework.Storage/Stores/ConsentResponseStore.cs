using IdentityServer4.Storage.Stores;
using Newtonsoft.Json;

namespace IdentityServer4.EntityFramework.Storage.Stores;

public sealed record ConsentResponseData(
    // Guid LoginRequestId,
    // Guid LoginResponseId,
    Guid ConsentRequestId
);

public class ConsentResponseStore : IConsentResponse2Store
{
    private readonly IStore _store;

    public ConsentResponseStore(IStore store)
    {
        _store = store;
    }

    public async Task<ConsentResponse2> Get(Guid id, CancellationToken ct)
    {
        var storeItemKey = BuildKey(id);
        var storeItem = await _store.Get(storeItemKey, ct);
        var data = JsonConvert.DeserializeObject<ConsentResponseData>(storeItem.Value) ??
                   throw new ApplicationException();
        var consentResponse = new ConsentResponse2(
            Id: id,
            ConsentRequestId: data.ConsentRequestId,
            CreatedAtUtc: storeItem.CreatedAtUtc,
            RemoveAtUtc: storeItem.RemoveAtUtc
        );
        return consentResponse;
    }

    public async Task Create(ConsentResponse2 consentResponse, CancellationToken ct)
    {
        var storeItemKey = BuildKey(consentResponse.Id);
        var data = new ConsentResponseData(
            ConsentRequestId: consentResponse.ConsentRequestId
        );
        var value = JsonConvert.SerializeObject(data);
        var storeItem = new StoreItem(
            Key: storeItemKey,
            Value: value,
            CreatedAtUtc: consentResponse.CreatedAtUtc,
            RemoveAtUtc: consentResponse.RemoveAtUtc
        );
        await _store.Create(storeItem, ct);
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        var storeItemKey = BuildKey(id);
        await _store.Delete(storeItemKey, ct);
    }

    private static string BuildKey(Guid id) => $"{id:N}_consentresponse";
}