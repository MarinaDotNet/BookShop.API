namespace BookShop.API.DTOs.Catalog;

/// <summary>
/// Represents the shopping cart returned to the client.
/// </summary>
/// <param name="Id">
/// The unique identifier of the cart.
/// </param>
/// <param name="Items">
/// The list of books currently in the cart.
/// </param>
/// <param name="TotalPrice">
/// The total price of all items in the cart. Computed as the sum of 
/// <see cref="CartItemDto.Price"/> multiplied by <see cref="CartItemDto.Quantity"/> for each item.  
/// </param>
public sealed record CartDto(string? Id, List<CartItemDto>Items, decimal TotalPrice);