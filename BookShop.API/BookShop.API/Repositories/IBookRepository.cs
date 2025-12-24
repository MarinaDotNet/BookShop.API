using BookShop.API.Models;

namespace BookShop.API.Repositories;

/// <summary>
/// Represents a repository that provides asynchronous access to book data.
/// </summary>
public interface IBookRepository
{
    /// <summary>
    /// Asynchronously retrieves all books from the data source.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all books. The
    /// collection is empty if no books are found.</returns>
    Task<IEnumerable<Book>> GetAllBooksAsync();
}
