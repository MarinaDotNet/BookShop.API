using Microsoft.Extensions.Options;
using BookShop.API.Models.Catalog;
using MongoDB.Driver;


namespace BookShop.API.Infrastructure.Persistence;

/// <summary>
/// MongoDB context for the carts collection, resolved from <see cref="CartMongoDbSettings"/>. 
/// </summary>
/// <remarks>
/// Delegates connection initialization and validation to <see cref="BaseMongoDbContext{T}"/>.
/// Exposes <see cref="GetCollection"/> publicly for use by the cart repository. 
/// </remarks>
public class CartMongoDbContext(IOptions<CartMongoDbSettings>settings):BaseMongoDbContext<Cart>(
    connectionString:settings.Value.ConnectionString, 
    databaseName:settings.Value.DatabaseName,
    collectionName: settings.Value.CollectionName
)
{
    /// <summary>
    /// Returns the MongoDB collection used to store and retrieve <see cref="Cart"/> documents. 
    /// </summary>
    public new IMongoCollection<Cart> GetCollection() => base.GetCollection();
}