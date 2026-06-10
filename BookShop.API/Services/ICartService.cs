using BookShop.API.DTOs.Catalog;

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
}