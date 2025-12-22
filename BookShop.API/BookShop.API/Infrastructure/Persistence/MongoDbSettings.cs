namespace BookShop.API.Infrastructure.Persistence;

/// <summary>
/// Represents the configuration settings required to connect to a MongoDB database.
/// </summary>
/// <remarks>Use this class to specify the connection string, database name, and collection name when configuring
/// access to a MongoDB instance. These settings are typically provided through application configuration files or
/// environment variables.</remarks>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = string.Empty;

    public string CollectionName { get; set; } = string.Empty;
}
