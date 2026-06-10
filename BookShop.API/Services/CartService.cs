using AutoMapper;
using BookShop.API.DTOs.Catalog;
using BookShop.API.Exceptions;
using BookShop.API.Models.Catalog;
using BookShop.API.Repositories;

namespace BookShop.API.Services;

/// <summary>
/// Implements <see cref="ICartService"/> using <see cref="ICartRepository"/> for data access and AutoMapper for model-to-DTO projection.  
/// </summary>
/// <param name="cartRepository">
/// The repository used to access cart data. Must not be null.
/// </param>
/// <param name="mapper">
/// The AutoMapper instance used to project cart models to DTOs. Must not be null.
/// </param>
public class CartService(ICartRepository cartRepository, IMapper mapper) : ICartService
{
    private readonly ICartRepository _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    /// <summary>
    /// Retrieves and maps the cart belonging to the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose cart to retrieve. Must not be null or whitespace.
    /// </param>
    /// <returns>
    /// The mapped <see cref="CartDto"/> if the cart exists; otherwise <c>null</c>. 
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="userId"/> is null or whitespace.
    /// </exception>
    public async Task<CartDto?> GetByUserIdAsync(string userId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(userId);

        var cart = await _cartRepository.GetByUserIdAsync(userId);

        if(cart is null)
        {
            return null;
        }

        return _mapper.Map<Cart, CartDto>(cart);
    }

    /// <summary>
    /// Creates and maps the cart for the specified user.
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
    /// <exception cref="ConflictException">
    /// Thrown when cart already exists for the specified user.
    /// </exception>
    public async Task<CartDto> CreateAsync(string userId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(userId);

        if (await IsCartExistsAsync(userId))
        {
            throw new ConflictException(nameof(userId));
        }

        DateTime currentTime = DateTime.UtcNow;

        Cart cart = new()
        {
            UserId = userId,
            Items = [],
            CreatedAt = currentTime,
            UpdatedAt = currentTime
        };

        await _cartRepository.CreateAsync(cart);
        return _mapper.Map<CartDto>(cart);
    }

    /// <summary>
    /// Checks if the specified user already has the cart.
    /// </summary>
    /// <param name="userId">
    /// The identifier of user for whom to check if cart exists or not.
    /// </param>
    /// <returns>
    /// The <c>true</c> if cart exists; otherwise <c>false</c>
    /// </returns>
    private async Task<bool> IsCartExistsAsync(string userId)
    {
        return await _cartRepository.GetByUserIdAsync(userId) is not null;
    }
}