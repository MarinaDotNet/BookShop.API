using BookShop.API.Exceptions;
using BookShop.API.Infrastructure.Persistence;
using BookShop.API.Models.Auth;
using BookShop.API.Models.Catalog;
using MongoDB.Driver;

namespace BookShop.API.Repositories;

public class CartRepository(CartMongoDbContext context) : ICartRepository
{
    private readonly IMongoCollection<Cart> _cartCollection = context.GetCollection();

    /// <summary>
    /// Retrieves the cart belonging to the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose cart to retrieve.
    /// </param>
    /// <returns>
    /// The cart if found; otherwise <c>null</c>.
    /// </returns>
    public async Task<Cart?> GetByUserIdAsync(string userId)
    {
        return await _cartCollection.Find(c => c.UserId == userId).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Persists a new cart document to the database.
    /// </summary>
    /// <param name="cart">
    /// The cart to create. Must not be null.
    /// </param>
    /// <returns>
    /// The created cart as stored in the database.
    /// </returns>
    public async Task<Cart> CreateAsync(Cart cart)
    {
        await _cartCollection.InsertOneAsync(cart);
        return cart;
    }

    /// <summary>
    /// Appends an item to the cart of the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose cart to update.
    /// </param>
    /// <param name="item">
    /// The item to add. Must not be null.
    /// </param>
    /// <returns>
    /// The updated cart if the user's cart exists; otherwise <c>null</c>.
    /// </returns>
    public async Task<Cart?> AddItemAsync(string userId, Item item)
    {
        var filter = Builders<Cart>
            .Filter
            .Eq(c => c.UserId, userId);

        var update = Builders<Cart>
            .Update
            .Push(c => c.Items, item)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<Cart>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _cartCollection.FindOneAndUpdateAsync(filter, update, options);
    }

    /// <summary>
    /// Updates the quantity of the specific item in the user's cart.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose cart to update.
    /// </param>
    /// <param name="bookId">
    /// The identifier of the book whose quantity to change.
    /// </param>
    /// <param name="quantity">
    /// The new quantity. Must be greater than zero.
    /// </param>
    /// <returns>
    /// The updated cart if both the cart and the item exist; otherwise <c>null</c>.
    /// </returns>
    public async Task<Cart?> UpdateItemQuantityAsync(string userId, string bookId, int quantity)
    {
        var filter = Builders<Cart>.Filter.And(
            Builders<Cart>.Filter.Eq(c => c.UserId, userId),
            Builders<Cart>.Filter.ElemMatch(c => c.Items, i => i.BookId == bookId)
        );

        var update = Builders<Cart>
            .Update
            .Set(c => c.Items[-1].Quantity, quantity)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<Cart>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _cartCollection.FindOneAndUpdateAsync(filter, update, options);
    }
    
    /// <summary>
    /// Removes a specific item from the user's cart.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose cart to update.
    /// </param>
    /// <param name="bookId">
    /// The identifier of the book to remove.
    /// </param>
    /// <returns>
    /// The updated cart if the cart exist; otherwise <c>null</c>.
    /// </returns>
    public async Task<Cart?> RemoveItemAsync(string userId, string bookId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes all items from the user's cart.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose cart to clear.
    /// </param>
    /// <returns>
    /// <c>true</c> if the cart was found and cleared; otherwise <c>false</c>.
    /// </returns>
    public async Task<bool> ClearAsync(string userId)
    {
        throw new NotImplementedException();
    }
}