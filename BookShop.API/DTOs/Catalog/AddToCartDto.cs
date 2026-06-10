namespace BookShop.API.DTOs.Catalog;

/// <summary>
/// Represents the data required to add a book to the shopping cart.
/// </summary>
/// <param name="BookId">
/// The identifier of the book to add. Must not be null or whitespace.
/// </param>
/// <param name="Quantity">
/// The number of copies to add. Must be greater than zero.
/// </param>
public sealed record AddToCartDto(string BookId, int Quantity);