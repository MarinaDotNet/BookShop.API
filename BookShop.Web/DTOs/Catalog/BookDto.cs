namespace BookShop.Web.DTOs.Catalog;

/// <summary>
/// Represents a book returned by the BookShop API.
/// </summary>
public sealed record BookDto(string Id, string Title, List<string> Authors, decimal Price, int Pages, string Publisher, string Language, List<string> Genres, Uri Link, bool IsAvailable, string Annotation);
