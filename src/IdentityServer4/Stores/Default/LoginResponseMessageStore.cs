using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Stores;

internal class LoginResponseMessageStore : ILoginResponseMessageStore
{
    protected readonly MessageCookie<LoginResponse> Cookie;

    public LoginResponseMessageStore(MessageCookie<LoginResponse> cookie)
    {
        Cookie = cookie;
    }

    public virtual Task DeleteAsync(string id)
    {
        Cookie.Clear(id);
        return Task.CompletedTask;
    }

    public virtual Task<Message<LoginResponse>> ReadAsync(string id)
    {
        return Task.FromResult(Cookie.Read(id));
    }

    public virtual Task WriteAsync(string id, Message<LoginResponse> message)
    {
        Cookie.Write(id, message);
        return Task.CompletedTask;
    }
}