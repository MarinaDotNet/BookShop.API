namespace BookShop.API.DTOs.Catalog;

/// <summary>
/// Data Transfer Object (DTO) for partially updating a book.
/// This DTO is used in HTTP PATCH requests to update only specific fields of an existing book.
/// </summary>
/// <param name="Id">
/// The unique identifier of the book to update. This field is required and must match the ID of the book being updated.
/// </param>
/// <param name="Title">
/// The new title of the book. This field is optional; if not provided, the title will remain unchanged. If provided, it must not be
/// empty and must not exceed 200 characters in length.
/// </param>
/// <param name="Authors">
/// The new list of authors for the book. This field is optional; if not provided, the authors will remain unchanged. If provided, the
/// list must not be empty, and each author name must not be null, empty, or whitespace.
/// </param>
/// <param name="Price">
/// The new price of the book. This field is optional; if not provided, the price will remain unchanged. If provided, it must be greater
/// than or equal to 0 and less than or equal to the maximum decimal value.
/// </param>
/// <param name="Pages">
/// The new number of pages in the book. This field is optional; if not provided, the page count will remain unchanged. If provided,
/// it must be a positive integer greater than or equal to 1 and less than or equal to the maximum integer value.
/// </param>
/// <param name="Publisher">
/// The new publisher of the book. This field is optional; if not provided, the publisher will remain unchanged. If provided, it must not
/// be empty and must not exceed 100 characters in length.
/// </param>
/// <param name="Language">
/// The new language of the book. This field is optional; if not provided, the language will remain unchanged. If provided, it must not
/// be empty and must not exceed 50 characters in length.
/// </param>
/// <param name="Genres">
/// The new list of genres for the book. This field is optional; if not provided, the genres will remain unchanged. If provided, the list
/// must not be empty, and each genre must not be null, empty, or whitespace.
/// </param>
/// <param name="Link">
/// The new link to the book's online resource. This field is optional; if not provided, the link will remain unchanged. If provided,
/// it must be a valid URI.
/// </param>
/// <param name="IsAvailable">
/// The new availability status of the book. This field is optional; if not provided, the availability will remain unchanged. If provided,
/// it indicates whether the book is currently available for purchase.
/// </param>
/// <param name="Annotation">
/// The new annotation or description of the book. This field is optional; if not provided, the annotation will remain unchanged. If provided,
/// it must not be empty and must not exceed 10,000 characters in length.
/// </param>
public sealed record BookUpdatePartlyDto(string Id, string? Title, List<string>? Authors, decimal? Price, int? Pages, string? Publisher, string? Language, List<string>? Genres, Uri? Link, bool? IsAvailable, string? Annotation);
