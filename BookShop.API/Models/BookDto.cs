namespace BookShop.API.Models;

/// <summary>
/// Data Transfer Object (DTO) representing a book.
/// </summary>
public sealed record BookDto(string Id, string Title, List<string> Authors, decimal Price, int Pages, string Publisher, string Language, List<string> Genres, Uri Link, bool IsAvailable, string Annotation);
