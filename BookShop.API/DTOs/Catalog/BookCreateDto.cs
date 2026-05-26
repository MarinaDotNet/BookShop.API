namespace BookShop.API.DTOs.Catalog;

/// <summary>
/// Data Transfer Object (DTO) for crating a new book.
/// This DTO is used in HTTP PUT requests to create new book.
/// </summary>
public sealed record BookCreateDto(string Id, string Title, List<string> Authors, decimal Price, int Pages, string Publisher, string Language, List<string> Genres, Uri Link, bool IsAvailable, string Annotation);
