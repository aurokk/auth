using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Stores;

/// <summary>
/// Interface for login messages that are sent from the login UI to the authorization endpoint.
/// </summary>
public interface ILoginResponseMessageStore
{
    /// <summary>
    /// Writes the login response message.
    /// </summary>
    /// <param name="id">The id for the message.</param>
    /// <param name="message">The message.</param>
    Task WriteAsync(string id, Message<LoginResponse> message);

    /// <summary>
    /// Reads the login response message.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    Task<Message<LoginResponse>> ReadAsync(string id);

    /// <summary>
    /// Deletes the login response message.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    Task DeleteAsync(string id);
}