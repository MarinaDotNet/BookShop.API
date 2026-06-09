namespace BookShop.API.DTOs.Catalog;

/// <summary>
/// Represents a single book entry within a <see cref="CartDto"/>. 
/// </summary>
/// <param name="BookId">
/// The identifier of the referenced book.
/// </param>
/// <param name="Title">
/// The title of the book.
/// </param>
/// <param name="Authors">
/// The authors of the book.
/// </param>
/// <param name="Price">
/// The price per copy as snapshotted when the book was added.
/// </param>
/// <param name="Quantity">
/// The number of copies requested.
/// </param>
public sealed record CartItemDto(string BookId, string Title, List<string>Authors, decimal Price, int Quantity);