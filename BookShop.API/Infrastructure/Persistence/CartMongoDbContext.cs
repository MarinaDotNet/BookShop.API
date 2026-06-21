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
public class CartMongoDbContext:BaseMongoDbContext<Cart>
{
    public CartMongoDbContext(IOptions<CartMongoDbSettings> settings) : base(settings.Value.ConnectionString, settings.Value.DatabaseName, settings.Value.CollectionName)
    {
        var indexKeys = Builders<Cart>.IndexKeys.Ascending(c => c.UserId);
        var indexOptions = new CreateIndexOptions { Unique = true };
        GetCollection().Indexes.CreateOne(new CreateIndexModel<Cart>(indexKeys, indexOptions));
    }
    
    /// <summary>
    /// Returns the MongoDB collection used to store and retrieve <see cref="Cart"/> documents. 
    /// </summary>
    public new IMongoCollection<Cart> GetCollection() => base.GetCollection();
}