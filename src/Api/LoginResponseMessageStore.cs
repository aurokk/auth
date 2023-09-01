using System.Collections.Concurrent;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace Api;

internal sealed class LoginResponseMessageStore : ILoginResponseMessageStore
{
    private readonly ConcurrentDictionary<string, Message<LoginResponse>> _data = new();

    public Task<Message<LoginResponse>?> ReadAsync(string id)
    {
        _data.TryGetValue(id, out var value);
        return Task.FromResult(value);
    }

    public Task WriteAsync(string id, Message<LoginResponse> message)
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