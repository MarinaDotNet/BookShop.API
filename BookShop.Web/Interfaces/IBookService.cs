namespace BookShop.Web.Interfaces;

using BookShop.Web.DTOs.Catalog;

/// <summary>
/// Provides methods for retrieving book data from the BookShop API
/// </summary>
public interface IBookService
{
    /// <summary>
    /// Retrieves the cheapest books.
    /// </summary>
    /// <param name="count">
    /// Number of books to retrieve. Must be between 10 and 50.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only collection for the cheapest books.
    /// </returns>
    Task<IReadOnlyCollection<BookDto>> GetTopCheapestBooksAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the most expensive books.
    /// </summary>
    /// <param name="count">
    /// Number of books to retrieve. Must be between 10 and 50.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only collection for the most expensive books.
    /// </returns>
    Task<IReadOnlyCollection<BookDto>> GetTopExpensiveBooksAsync(int count, CancellationToken cancellationToken = default);
}