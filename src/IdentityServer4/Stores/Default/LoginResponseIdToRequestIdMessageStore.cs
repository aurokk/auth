using IdentityServer4.Models;

namespace IdentityServer4.Stores;

internal class LoginResponseIdToRequestIdMessageStore : ILoginResponseIdToRequestIdMessageStore
{
    protected readonly MessageCookie<LoginResponseIdToRequestId> Cookie;

    public LoginResponseIdToRequestIdMessageStore(MessageCookie<LoginResponseIdToRequestId> cookie)
    {
        Cookie = cookie;
    }

    public virtual Task DeleteAsync(string id)
    {
        Cookie.Clear(id);
        return Task.CompletedTask;
    }

    public virtual Task<Message<LoginResponseIdToRequestId>> ReadAsync(string id)
    {
        return Task.FromResult(Cookie.Read(id));
    }

    public virtual Task WriteAsync(string id, Message<LoginResponseIdToRequestId> message)
    {
        Cookie.Write(id, message);
        return Task.CompletedTask;
    }
}