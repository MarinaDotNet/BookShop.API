using AutoMapper;
using BookShop.API.DTOs.Catalog;
using BookShop.API.Exceptions;
using BookShop.API.Models.Catalog;
using BookShop.API.Repositories;
using MongoDB.Bson;

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
/// <param name="bookRepository">
/// The repository used to acces book data. Must not be null.
/// </param>
public class CartService(ICartRepository cartRepository, IBookRepository bookRepository, IMapper mapper) : ICartService
{
    private readonly ICartRepository _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
    private readonly IBookRepository _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));

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
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="userId"/> is null or whitespace.
    /// </exception>
    public async Task<CartDto?> GetByUserIdAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

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
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

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
    public async Task<CartDto> AddItemAsync(string userId, AddToCartDto addToCart)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(addToCart);
        ArgumentException.ThrowIfNullOrWhiteSpace(addToCart.BookId);

        var cart = await GetOrCreateCartAsync(userId);

        var book = await _bookRepository.GetBookByIdAsync(addToCart.BookId)
            ?? throw new NotFoundException(nameof(Book));

        if (!book.IsAvailable)
        {
            throw new ValidationException(nameof(book.IsAvailable));
        }

        if(cart.Items.Any(i => i.BookId == book.Id))
        {
            int newQuantity = cart.Items.First(i => i.BookId == book.Id).Quantity + addToCart.Quantity;

            cart = await _cartRepository.UpdateItemQuantityAsync(userId, book.Id!, newQuantity) ?? cart;
        }
        else
        {
           cart = await _cartRepository.AddItemAsync(userId, BuildCartItem(book, addToCart.Quantity)) ?? cart;
        }

        return _mapper.Map<CartDto>(cart);
    }

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
    public async Task<CartDto> UpdateItemQuantityAsync(string userId, string bookId, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(bookId);
        if(quantity <= 0)
        {
            throw new ArgumentException("The quantity should be greater than zero.", nameof(quantity));
        }

        var cart = await _cartRepository.UpdateItemQuantityAsync(userId, bookId, quantity)
            ?? throw new NotFoundException("Cart or item not found.");

        return _mapper.Map<CartDto>(cart);
    }

    /// <summary>
    /// Removes a specific item form the shopping cart of the currently authenticated user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose cart to updated. Must not be null or whitespace.
    /// </param>
    /// <param name="bookId">
    /// The identifier of the book to remove from the cart. Must not be null or whitespace.
    /// </param>
    /// <returns>
    /// The mapped <see cref="CartDto"/> without the specified <paramref name="bookId"/> item. 
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when the cart does not exists.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="userId"/> or <paramref name="bookId"/> is null or whitespace.
    /// </exception> 
    public async Task<CartDto> RemoveItemAsync(string userId, string bookId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(bookId);

        var cart = await _cartRepository.RemoveItemAsync(userId, bookId)
            ?? throw new NotFoundException("Cart or item not found.");

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

    /// <summary>
    /// Returns the existing cart for the user, or creates and returns a new one.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier currently authenticated user, whose cart to return.
    /// </param>
    /// <returns>
    /// The existing or new cart of authenticated user.
    /// </returns>
    private async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        return await _cartRepository.GetByUserIdAsync(userId)
            ?? await _cartRepository.CreateAsync(new Cart
            {
               UserId = userId,
               Items = [],
               CreatedAt = DateTime.UtcNow,
               UpdatedAt = DateTime.UtcNow 
            });
    }

    /// <summary>
    /// Creates a cart item snapshot from the given <paramref name="book"/> and <paramref name="quantity"/>.
    /// </summary>
    /// <param name="book">
    /// The book to create the snapshot from.
    /// </param>
    /// <param name="quantity">
    /// The quantity of <paramref name="book"/> items.
    /// </param>
    /// <returns>
    /// Snapshot from the <paramref name="book"/> with quantity <paramref name="quantity"/>.
    /// </returns>
    private static Item BuildCartItem(Book book, int quantity)
    {
        return new()
        {
            BookId = book.Id,
            Title = book.Title,
            Authors = book.Authors ?? [],
            Price = book.Price,
            Quantity = quantity
        };
    }
}