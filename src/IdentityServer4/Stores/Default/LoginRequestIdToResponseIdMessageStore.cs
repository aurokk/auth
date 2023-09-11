using IdentityServer4.Models;

namespace IdentityServer4.Stores;

internal class LoginRequestIdToResponseIdMessageStore : ILoginRequestIdToResponseIdMessageStore
{
    protected readonly MessageCookie<LoginRequestIdToResponseId> Cookie;

    public LoginRequestIdToResponseIdMessageStore(MessageCookie<LoginRequestIdToResponseId> cookie)
    {
        Cookie = cookie;
    }

    public virtual Task DeleteAsync(string id)
    {
        Cookie.Clear(id);
        return Task.CompletedTask;
    }

    public virtual Task<Message<LoginRequestIdToResponseId>> ReadAsync(string id)
    {
        return Task.FromResult(Cookie.Read(id));
    }

    public virtual Task WriteAsync(string id, Message<LoginRequestIdToResponseId> message)
    {
        Cookie.Write(id, message);
        return Task.CompletedTask;
    }
}