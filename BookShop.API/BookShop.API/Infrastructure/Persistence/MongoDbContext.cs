using BookShop.API.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookShop.API.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoCollection<Book> _collection;
    private readonly MongoClient _client;

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
