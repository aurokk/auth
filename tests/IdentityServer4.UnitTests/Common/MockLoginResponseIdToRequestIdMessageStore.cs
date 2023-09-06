using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace IdentityServer.UnitTests.Common;

public class MockLoginResponseIdToRequestIdMessageStore : ILoginResponseIdToRequestIdMessageStore
{
    private readonly ConcurrentDictionary<string, Message<LoginResponseIdToRequestId>> _messages = new();

    public Task DeleteAsync(string id)
    {
        _messages.Remove(id, out _);
        return Task.CompletedTask;
    }

    public Task<Message<LoginResponseIdToRequestId>> ReadAsync(string id)
    {
        _messages.TryGetValue(id, out var message);
        return Task.FromResult(message);
    }

    public Task WriteAsync(string id, Message<LoginResponseIdToRequestId> message)
    {
        _messages[id] = message;
        return Task.CompletedTask;
    }
}