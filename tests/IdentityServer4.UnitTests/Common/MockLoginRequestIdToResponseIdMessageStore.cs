using System.Collections.Concurrent;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace IdentityServer.UnitTests.Common;

public class MockLoginRequestIdToResponseIdMessageStore : ILoginRequestIdToResponseIdMessageStore
{
    private readonly ConcurrentDictionary<string, Message<LoginRequestIdToResponseId>> _messages = new();

    public Task DeleteAsync(string id)
    {
        _messages.Remove(id, out _);
        return Task.CompletedTask;
    }

    public Task<Message<LoginRequestIdToResponseId>> ReadAsync(string id)
    {
        _messages.TryGetValue(id, out var message);
        return Task.FromResult(message);
    }

    public Task WriteAsync(string id, Message<LoginRequestIdToResponseId> message)
    {
        _messages[id] = message;
        return Task.CompletedTask;
    }
}