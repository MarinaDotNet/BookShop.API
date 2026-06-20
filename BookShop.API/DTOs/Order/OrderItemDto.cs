namespace BookShop.API.DTOs.Order;

/// <summary>
/// Represents a single item within an order, capturing a snapshot of the book details at the time the order was placed.
/// </summary>
/// <param name="BookId">
/// The identifier of the book.
/// </param>
/// <param name="Title">
/// The title of the book at the time the order was placed.
/// </param>
/// <param name="Authors">
/// The list of authors of the book at the time the order was placed.
/// </param>
/// <param name="Price">
/// The price of a single unit at the time the order was placed.
/// </param>
/// <param name="Quantity">
/// The number of units ordered.
/// </param>
public sealed record OrderItemDto(string BookId, string Title, List<string>Authors, decimal Price, int Quantity);