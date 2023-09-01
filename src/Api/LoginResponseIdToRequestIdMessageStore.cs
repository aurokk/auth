using System.Collections.Concurrent;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace Api;

internal class LoginResponseIdToRequestIdMessageStore : ILoginResponseIdToRequestIdMessageStore
{
    private readonly ConcurrentDictionary<string, Message<LoginResponseIdToRequestId>> _data = new();

    public Task<Message<LoginResponseIdToRequestId>?> ReadAsync(string id)
    {
        _data.TryGetValue(id, out var value);
        return Task.FromResult(value);
    }

    public Task WriteAsync(string id, Message<LoginResponseIdToRequestId> message)
    {
        _data[id] = message;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        _data.Remove(id, out _);
        return Task.CompletedTask;
    }
}