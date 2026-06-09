using BookShop.API.Models.Catalog;

namespace BookShop.API.Repositories;

/// <summary>
/// Defines data access operations for cart documents in MongoDB.
/// </summary>
public interface ICartRepository
{
    /// <summary>
    /// Retrieves the cart belonging to the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose cart to retrieve.
    /// </param>
    /// <returns>
    /// The cart if found; otherwise <c>null</c>.
    /// </returns>
    Task<Cart?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Persists a new cart document to the database.
    /// </summary>
    /// <param name="cart">
    /// The cart to create. Must not be null.
    /// </param>
    /// <returns>
    /// The created cart as stored in the database.
    /// </returns>
    Task<Cart> CreateAsync(Cart cart);

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
    Task<Cart?> AddItemAsync(string userId, Item item);

    /// <summary>
    /// Removes a specific item from the user's cart.
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
    Task<Cart?> UpdateItemQuantityAsync(string userId, string bookId, int quantity);
    
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
    Task<Cart?> RemoveItemAsync(string userId, string bookId);

    /// <summary>
    /// Removes all items from the user's cart.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose cart to clear.
    /// </param>
    /// <returns>
    /// <c>true</c> if the cart was found and cleared; otherwise <c>false</c>.
    /// </returns>
    Task<bool> ClearAsync(string userId);
}