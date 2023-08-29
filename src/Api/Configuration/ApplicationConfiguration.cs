namespace Api.Configuration;

public class ApplicationConfiguration
{
    public DatabaseConfiguration Database { get; }
    public AuthenticationConfiguration Authentication { get; }
    public SendGridConfiguration SendGrid { get; }

    public ApplicationConfiguration(
        DatabaseConfiguration database,
        AuthenticationConfiguration authentication,
        SendGridConfiguration sendGrid)
    {
        Database = database;
        Authentication = authentication;
        SendGrid = sendGrid;
    }
}