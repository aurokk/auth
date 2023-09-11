using IdentityServer4.Models;

namespace IdentityServer4.Stores;

public interface ILoginResponseIdToRequestIdMessageStore
{
    Task WriteAsync(string id, Message<LoginResponseIdToRequestId> message);
    Task<Message<LoginResponseIdToRequestId>> ReadAsync(string id);
    Task DeleteAsync(string id);
}