using BookShop.API.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookShop.API.Infrastructure.Persistence;

/// <summary>
/// Provides a MongoDB context for accessing and managing a collection of books using the specified database settings.
/// </summary>
/// <remarks>This context encapsulates the connection to a MongoDB database and exposes access to a single
/// collection of books. It is typically used to centralize MongoDB configuration and collection access within an
/// application. The context is initialized with validated settings and throws exceptions if the configuration is
/// invalid or the connection cannot be established.</remarks>
public class MongoDbContext
{
    private readonly IMongoCollection<Book> _collection;
    private readonly MongoClient _client;

    /// <summary>
    /// Gets the MongoDB collection used to store and retrieve Book entities.
    /// </summary>
    /// <returns>An <see cref="IMongoCollection{Book}"/> representing the collection of books in the database.</returns>
    public IMongoCollection<Book> GetCollection() => _collection;

    /// <summary>
    /// Initializes a new instance of the MongoDbContext class using the specified MongoDB settings.
    /// </summary>
    /// <remarks>This constructor validates the provided settings and establishes a connection to the
    /// specified MongoDB database and collection. If the configuration is invalid or the connection cannot be
    /// established, an exception is thrown.</remarks>
    /// <param name="settings">The MongoDB configuration settings. Must not be null and must contain valid connection information.</param>
    /// <exception cref="ArgumentNullException">Thrown if settings or its Value property is null.</exception>
    /// <exception cref="MongoConfigurationException">Thrown if the MongoDB connection or collection cannot be initialized due to invalid configuration or connection
    /// failure.</exception>
    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        if (settings is null || settings.Value is null)
        {
            throw new ArgumentNullException(nameof(settings), "MongoDB settings cannot be null.");
        }

        var mongoDbSettings = settings.Value;

        ValidateMongoDBSettings(mongoDbSettings);

        try
        {
            _client = new MongoDB.Driver.MongoClient(mongoDbSettings.ConnectionString);
            var mongoDB = _client.GetDatabase(mongoDbSettings.DatabaseName);
            _collection = mongoDB.GetCollection<Book>(mongoDbSettings.CollectionName);
        }
        catch (Exception ex)
        {
            throw new MongoConfigurationException("Failed to initialize MongoDB connection.", ex);
        }
    }

    /// <summary>
    /// Validates that the specified MongoDB settings contain non-empty values for the connection string, database name,
    /// and collection name.
    /// </summary>
    /// <param name="settings">The MongoDB settings to validate. Must not be null and must contain valid values for all required properties.</param>
    /// <exception cref="ArgumentException">Thrown if the connection string, database name, or collection name in <paramref name="settings"/> is null,
    /// empty, or consists only of white-space characters.</exception>
    private static void ValidateMongoDBSettings(MongoDbSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new ArgumentException("MongoDB connection string cannot be null or empty.",  nameof(settings));
        }
        if (string.IsNullOrWhiteSpace(settings.DatabaseName))
        {
            throw new ArgumentException("MongoDB database name cannot be null or empty.", nameof(settings));
        }
        if (string.IsNullOrWhiteSpace(settings.CollectionName))
        {
            throw new ArgumentException("MongoDB collection name cannot be null or empty.", nameof(settings));
        }
    }
}
