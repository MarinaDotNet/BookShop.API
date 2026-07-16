using BookShop.Web.DTOs.Catalog;
using BookShop.Web.Interfaces;
using BookShop.Web.Constants;


namespace BookShop.Web.Services;

/// <summary>
/// Provides a book-related operations by communicating with the BookShop API.
/// </summary>
public class BookService(IApiClient apiClient) : IBookService
{
    private readonly IApiClient _apiClient = apiClient
        ?? throw new ArgumentNullException(nameof(apiClient));

    /// <summary>
    /// Retrieves the specified number of the cheapest books.
    /// </summary>
    /// <param name="count">
    /// Number of books to retrieve. Must be between 10 and 50.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only collection of the books.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="count"/> is outside the allowed range.
    /// </exception>
    public async Task<IReadOnlyCollection<BookDto>>GetTopCheapestBooksAsync(int count, CancellationToken cancellationToken = default)
    {
        if(count is < 10 or > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 10 and 50.");
        }

        var result = await _apiClient.GetAsync<IReadOnlyCollection<BookDto>>(
            $"{ApiRoutes.V3.Books.Cheapest}?count={count}", 
            cancellationToken);
        return result ?? [];
    }

    /// <summary>
    /// Retrieves the specified number of the most expensive books.
    /// </summary>
    /// <param name="count">
    /// Number of books to retrieve. Must be between 10 and 50.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only collection of the books.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="count"/> is outside the allowed range.
    /// </exception>
    public async Task<IReadOnlyCollection<BookDto>>GetTopExpensiveBooksAsync(int count, CancellationToken cancellationToken = default)
    {
        if(count is < 10 or > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 10 and 50.");
        }

        var result = await _apiClient.GetAsync<IReadOnlyCollection<BookDto>>(
            $"{ApiRoutes.V3.Books.Expensive}?count={count}", 
            cancellationToken);
        return result ?? [];
    }
}