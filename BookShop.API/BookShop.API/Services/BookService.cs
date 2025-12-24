using BookShop.API.Models;
using BookShop.API.Repositories;

namespace BookShop.API.Services;

/// <summary>
/// Provides operations for retrieving book data from a repository.
/// </summary>
/// <remarks>This service acts as an abstraction over the underlying book repository, allowing consumers to
/// retrieve book information asynchronously. Thread safety depends on the implementation of the provided
/// repository.</remarks>
/// <param name="bookRepository">The repository used to access book data. Cannot be null.</param>
public class BookService(IBookRepository bookRepository)
{
    private readonly IBookRepository _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));

    /// <summary>
    /// Asynchronously retrieves all books from the data store.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see
    /// cref="Book"/> objects representing all books. If no books are found, the collection will be empty.</returns>
    public async Task <IEnumerable<Book>> GetAllBooksAsync()
    {
        return await _bookRepository.GetAllBooksAsync();
    }
}
