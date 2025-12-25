using BookShop.API.Infrastructure.Persistence;
using BookShop.API.Models;
using MongoDB.Driver;
using System.Runtime.CompilerServices;

namespace BookShop.API.Repositories;

/// <summary>
/// Provides access to book data stored in a MongoDB database.
/// </summary>
/// <remarks>This repository implements data access operations for books using MongoDB. All methods are
/// asynchronous and thread-safe. The repository should be disposed of when no longer needed to release database
/// resources.</remarks>
/// <param name="context">The MongoDB context used to access the book collection. Cannot be null.</param>
public class BookRepository(MongoDbContext context) : IBookRepository
{
    /// <summary>
    /// Represents the MongoDB collection used to store and retrieve <see cref="Book"/> entities.
    /// </summary>
    private readonly IMongoCollection<Book> _booksCollection = context.GetCollection();

    /// <summary>
    /// Asynchronously retrieves all books from the data store.
    /// </summary>
    /// <remarks>This method performs a read operation that returns all available books. The returned
    /// collection reflects the current state of the data store at the time of the query. The order of books in the
    /// collection is not guaranteed.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of all <see
    /// cref="Book"/> objects in the data store. If no books are found, the collection will be empty.</returns>
    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        var books = await _booksCollection.Find(_ => true).ToListAsync();
        return books;
    }

    /// <summary>
    /// Asynchronously retrieves a book with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the book to retrieve. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Book"/> with the
    /// specified identifier, or <see langword="null"/> if no matching book is found.</returns>
    public async Task<Book> GetBookByIdAsync(string id)
    {
        var book = await _booksCollection.Find(b => b.Id!.Equals(id)).FirstOrDefaultAsync();
        return book;
    }
}
