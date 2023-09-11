using System.Collections.Concurrent;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace IdentityServer.UnitTests.Common;

public class MockLoginResponseMessageStore : ILoginResponseMessageStore
{
    private readonly ConcurrentDictionary<string, Message<LoginResponse>> _messages = new();

    public Task DeleteAsync(string id)
    {
        if (id != null && _messages.ContainsKey(id))
        {
            _messages.Remove(id, out _);
        }

        return Task.CompletedTask;
    }

    public Task<Message<LoginResponse>> ReadAsync(string id)
    {
        Message<LoginResponse> val = null;
        if (id != null)
        {
            _messages.TryGetValue(id, out val);
        }

        return Task.FromResult(val);
    }

    public Task WriteAsync(string id, Message<LoginResponse> message)
    {
        _messages[id] = message;
        return Task.CompletedTask;
    }
}