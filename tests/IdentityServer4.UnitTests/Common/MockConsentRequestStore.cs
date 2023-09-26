using System.Collections.Concurrent;
using IdentityServer4.Storage.Stores;

namespace IdentityServer4.UnitTests.Common;

public class MockConsentRequestStore : IConsentRequest2Store
{
    private readonly ConcurrentDictionary<Guid, ConsentRequest2> _items = new();

    public Task<ConsentRequest2> Get(Guid id, CancellationToken ct)
    {
        _items.TryGetValue(id, out var consentRequest);
        return Task.FromResult(consentRequest);
    }

    public Task Create(ConsentRequest2 consentRequest, CancellationToken ct)
    {
        _items[consentRequest.Id] = consentRequest;
        return Task.CompletedTask;
    }

    public Task Delete(Guid id, CancellationToken ct)
    {
        _items.Remove(id, out _);
        return Task.CompletedTask;
    }
}