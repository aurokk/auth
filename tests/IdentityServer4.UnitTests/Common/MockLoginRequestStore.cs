using System.Collections.Concurrent;
using IdentityServer4.Storage.Stores;

namespace IdentityServer4.UnitTests.Common;

public class MockLoginRequestStore : ILoginRequestStore
{
    private readonly ConcurrentDictionary<Guid, LoginRequest> _items = new();

    public Task<LoginRequest> Get(Guid id, CancellationToken ct)
    {
        _items.TryGetValue(id, out var loginRequest);
        return Task.FromResult(loginRequest);
    }

    public Task Create(LoginRequest loginRequest, CancellationToken ct)
    {
        _items[loginRequest.Id] = loginRequest;
        return Task.CompletedTask;
    }

    public Task Delete(Guid id, CancellationToken ct)
    {
        _items.Remove(id, out _);
        return Task.CompletedTask;
    }
}