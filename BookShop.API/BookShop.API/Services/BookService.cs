using BookShop.API.Exceptions;
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
    /// <returns>A read-only collection containing all <see cref="Book"/> objects. The collection is empty if no books are found.</returns>
    public async Task <IReadOnlyCollection<Book>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllBooksAsync();

        if(!books.Any())
        {
            throw new NotFoundException("No books found.");
        }

        return [.. books ];
    }
}
