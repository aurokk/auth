namespace Api.Configuration;

public class SendGridConfiguration
{
    public string ApiKey { get; }

    public SendGridConfiguration(string apiKey)
    {
        ApiKey = apiKey;
    }
}