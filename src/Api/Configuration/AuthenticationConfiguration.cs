namespace Api.Configuration;

public class AuthenticationConfiguration
{
    public string Authority { get; }

    public AuthenticationConfiguration(string authority)
    {
        Authority = authority;
    }
}