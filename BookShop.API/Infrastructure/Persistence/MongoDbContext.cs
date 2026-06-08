using BookShop.API.Models.Catalog;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookShop.API.Infrastructure.Persistence;

/// <summary>
/// MongoDB context for the books collection, resolved from <see cref="MongoDbSettings"/>. 
/// </summary>
/// <remarks>
/// Deligates connection initialization and validation to <see cref="BaseMongoDbContext{T}"/>.
/// Exposes <see cref="GetCollection"/> publicly for use by the book repository. 
/// </remarks>
public class MongoDbContext(IOptions<MongoDbSettings> settings) : BaseMongoDbContext<Book>(
    connectionString:settings.Value.ConnectionString, 
    databaseName:settings.Value.DatabaseName,
    collectionName: settings.Value.CollectionName)
{
    /// <summary>
    /// Returns the MongoDB collection used to store and retrieve <see cref="Book"/> documents. 
    /// </summary>
    /// <returns></returns>
    public new IMongoCollection<Book> GetCollection() => base.GetCollection(); 
}
