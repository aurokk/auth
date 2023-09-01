using System.Collections.Concurrent;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace Api;

internal class LoginRequestIdToResponseIdMessageStore : ILoginRequestIdToResponseIdMessageStore
{
    private readonly ConcurrentDictionary<string, Message<LoginRequestIdToResponseId>> _data = new();

    public Task<Message<LoginRequestIdToResponseId>?> ReadAsync(string id)
    {
        _data.TryGetValue(id, out var value);
        return Task.FromResult(value);
    }

    public Task WriteAsync(string id, Message<LoginRequestIdToResponseId> message)
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