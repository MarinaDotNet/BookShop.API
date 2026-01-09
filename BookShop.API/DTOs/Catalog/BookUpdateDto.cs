namespace BookShop.API.DTOs.Catalog;

public sealed record BookUpdateDto(string Id, string? Title, List<string>? Authors, decimal? Price, int? Pages, string? Publisher, string? Language, List<string>? Genres, Uri? Link, bool? IsAvailable, string? Annotation);
