using MongoDB.Driver;

namespace BookShop.API.Infrastructure.Persistence;

/// <summary>
/// Abstract generic base class that manages a MongoDB connection and exposes access to a typed collection.
/// </summary>
/// <typeparam name="T">
/// The document type stored in the MongoDB collection.
/// </typeparam>
/// <remarks>
/// Derived classes are responsible for supplying connection parameters from their specific settings.
/// Validation and connection initialization are handled once in this base class to avoid duplication.
/// </remarks>
public abstract class BaseMongoDbContext<T>
{
    private readonly IMongoCollection<T> _collection;
    private readonly MongoClient _client;

    /// <summary>
    /// Returns the typed MongoDB collection initialized during construction.
    /// </summary>
    protected IMongoCollection<T> GetCollection() => _collection;

    /// <summary>
    /// Initialize the MongoDB client and collection using the provided connection parameters.
    /// </summary>
    /// <param name="connectionString">
    /// The MongoDB connection string. Must be not null or whitespace.
    /// </param>
    /// <param name="databaseName">
    /// The name of the target database. Must be not null or whitespace.
    /// </param>
    /// <param name="collectionName">
    /// The name of the target collection. Must be not null or whitespace.
    /// </param>
    /// <exception cref="MongoConfigurationException">
    /// Thrown if any parameter is null or whitespace, if the connction string is invalid, or if the connection cannot be established.
    /// </exception>
    protected BaseMongoDbContext(string connectionString, string databaseName, string collectionName)
    {
        ValidateMongoDBSettings(connectionString, databaseName, collectionName);

        try
        {
            _client = new MongoClient(connectionString);
            var mongoDB = _client.GetDatabase(databaseName);
            _collection = mongoDB.GetCollection<T>(collectionName);
        }
        catch(MongoConfigurationException ex)
        {
            throw new MongoConfigurationException("Failed to initialize MongoDB connection due to invalid configuration.", ex);
        }
        catch(Exception ex)
        {
            throw new MongoConfigurationException("Failed to initialize MongoDB connection due an unexpected error.", ex);
        }
    }

    private static void ValidateMongoDBSettings(string connectionString, string databaseName, string collectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new MongoConfigurationException("MongoDB connection string is not configured.");
        }
        if(string.IsNullOrWhiteSpace(databaseName))
        {
            throw new MongoConfigurationException("MongoDB database name is not configured.");
        }
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new MongoConfigurationException("MongoDB collection name is not configured.");
        }
    }
}