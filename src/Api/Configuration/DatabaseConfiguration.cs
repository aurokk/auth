namespace Api.Configuration;

public class DatabaseConfiguration
{
    public string ConnectionString { get; }

    public DatabaseConfiguration(string connectionString)
    {
        ConnectionString = connectionString;
    }
}