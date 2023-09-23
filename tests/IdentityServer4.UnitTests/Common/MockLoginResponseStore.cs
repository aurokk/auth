using System.Collections.Concurrent;
using IdentityServer4.Storage.Stores;

namespace IdentityServer4.UnitTests.Common;

public class MockLoginResponseStore : ILoginResponseStore
{
    private readonly ConcurrentDictionary<Guid, LoginResponse> _items = new();

    public Task<LoginResponse> Get(Guid id, CancellationToken ct)
    {
        _items.TryGetValue(id, out var loginResponse);
        return Task.FromResult(loginResponse);
    }

    public Task Create(LoginResponse loginResponse, CancellationToken ct)
    {
        _items[loginResponse.Id] = loginResponse;
        return Task.CompletedTask;
    }

    public Task Delete(Guid id, CancellationToken ct)
    {
        _items.Remove(id, out _);
        return Task.CompletedTask;
    }
}