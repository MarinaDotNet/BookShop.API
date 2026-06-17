using BookShop.API.DTOs.Catalog;
using BookShop.API.Exceptions;

namespace BookShop.API.Services;

/// <summary>
/// Defines business logic operations for managing shopping carts.
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Retrieves the cart belonging to the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose cart to retrieve. Must not be null or whitespace.
    /// </param>
    /// <returns>
    /// The mapped <see cref="CartDto"/> if the cart exists; otherwise <c>null</c>. 
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="userId"/> is null or whitespace.
    /// </exception> 
    Task<CartDto?> GetByUserIdAsync(string userId);

    /// <summary>
    /// Creates and maps the cart for the  specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user for whom to create new cart. Must not be null or whitespace.
    /// </param>
    /// <returns>
    /// The mapped <see cref="CartDto"/> if the cart created successfully.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="userId"/> is null or whitespace.
    /// </exception>
    Task<CartDto> CreateAsync(string userId);

    /// <summary>
    /// Adds requested item to the cart for the specified user if cart exists and requested item not in cart yet.
    /// If user does not have cart than creates new cart and adds to it requested item.
    /// If item already in the user's cart than increments the quantity of this item in cart.
    /// </summary>
    /// <param name="userId">
    /// The identifier of user into whose cart to add item. Must not be null or whitespace.
    /// </param>
    /// <param name="addToCart">
    /// The item which needs to be added into cart.
    /// </param>
    /// <returns>
    /// The mapped <see cref="CartDto"/> with added item if the item added successfully. 
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="userId"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="addToCart"/> is null.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown when the book with the specified ID does not exist.
    /// </exception>
    /// <exception cref="ValidationException">
    /// Thrown when the BookId format is invalid or the book is not available.
    /// </exception>
    Task<CartDto> AddItemAsync(string userId, AddToCartDto addToCart);

    /// <summary>
    /// Updates the quantity of a specific item in user's cart.
    /// </summary>
    /// <param name="userId">
    /// The identification of the user whose cart to update. Must not be null or whitespace.
    /// </param>
    /// <param name="bookId">
    /// The identifier of the book whose quantity to change. Must not be null or whitepace.
    /// </param>
    /// <param name="quantity">
    /// The new quantit. Must be greater than zero.
    /// </param>
    /// <returns>
    /// The mapped <see cref="CartDto"/> with the updated item quantity. 
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="bookId"/> or <paramref name="userId"/> is null or whitespace, or if <paramref name="quantity"/> 
    /// is less than or equal to zero.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown when the cart or the specified item does not exist.
    /// </exception>
    Task<CartDto> UpdateItemQuantityAsync(string userId, string bookId, int quantity);
}