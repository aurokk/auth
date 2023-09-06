using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Stores;

public interface ILoginRequestIdToResponseIdMessageStore
{
    Task WriteAsync(string id, Message<LoginRequestIdToResponseId> message);
    Task<Message<LoginRequestIdToResponseId>> ReadAsync(string id);
    Task DeleteAsync(string id);
}