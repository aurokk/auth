using System.Collections.Concurrent;
using IdentityServer4.Storage.Stores;

namespace IdentityServer4.UnitTests.Common;

public class MockConsentResponseStore : IConsentResponse2Store
{
    private readonly ConcurrentDictionary<Guid, ConsentResponse2> _items = new();

    public Task<ConsentResponse2> Get(Guid id, CancellationToken ct)
    {
        _items.TryGetValue(id, out var consentResponse);
        return Task.FromResult(consentResponse);
    }

    public Task Create(ConsentResponse2 consentResponse, CancellationToken ct)
    {
        _items[consentResponse.Id] = consentResponse;
        return Task.CompletedTask;
    }

    public Task Delete(Guid id, CancellationToken ct)
    {
        _items.Remove(id, out _);
        return Task.CompletedTask;
    }
}