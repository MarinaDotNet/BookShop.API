using BookShop.API.Infrastructure.Persistence;
using BookShop.API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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
    /// Asynchronously retrieves all books from the data source, with optional filter for availability.
    /// </summary>
    /// <remarks>
    /// This method performs a read operation that returns all available books, with optional 
    /// filter for availability. 
    /// The returned collection reflects the current state of the data store at the time of the query. 
    /// The order of books in the collection is not guaranteed.
    /// </remarks>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status. 
    /// If provided, only books match the specified availability status will be returned. 
    /// If null, no avialability filter is applied.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection 
    /// of books that match the availability filter, or all books if filter not applied. 
    /// The collection is empty if no books are found.
    /// </returns>
    public async Task<IReadOnlyCollection<Book>> GetAllBooksAsync(bool? isAvailable)
    {
        var filter = isAvailable.HasValue
            ? Builders<Book>.Filter.Eq(b => b.IsAvailable, isAvailable.Value)
            : Builders<Book>.Filter.Empty;

        var books = await _booksCollection.Find(filter).ToListAsync();
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

    /// <summary>
    /// Asynchronously retrieves <see cref="Book"/> documents from MongoDB that partially match the specified
    /// <paramref name="searchTerm"/> in one of the searchable fields 
    /// (Title, Authors, Publisher, Genres, Annotation) with optional filter for availability.
    /// </summary>
    /// <param name="searchTerm">
    /// The value to search for in the searchable fields.
    /// </param>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status.
    /// If null then no availability filter is applied.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of books.
    /// </returns>
    public async Task<IReadOnlyCollection<Book>> GetBooksByExactMatchAsync(string searchTerm, bool? isAvailable)
    {
        var trimedTerm = searchTerm.Trim();
        var escapedTerm = Regex.Escape(trimedTerm);
        var regex = new BsonRegularExpression($"^{escapedTerm}$", "i");

        var filters = new List<FilterDefinition<Book>>
        {
            Builders<Book>.Filter.Regex(b => b.Title, regex),
            Builders<Book>.Filter.Regex(b => b.Language, regex),
            Builders<Book>.Filter.Regex(b => b.Publisher, regex),

            Builders<Book>.Filter.Regex("Authors", regex),
            Builders<Book>.Filter.Regex("Genres", regex)
        };

        var searchFilter = Builders<Book>.Filter.Or(filters);

        if (isAvailable.HasValue)
        {
            var availabilityFilter =
            Builders<Book>.Filter.Eq(b => b.IsAvailable, isAvailable.Value);

            searchFilter = Builders<Book>.Filter.And(searchFilter, availabilityFilter);
        }

        return await _booksCollection.Find(searchFilter).ToListAsync();
    }

    /// <summary>
    /// Asynchronously retrieves <see cref="Book"/> documents from MongoDB that contain the specified
    /// <paramref name="searchTerm"/> as a partial, case-insensitive match in one of the searchable fields
    /// (Title, Authors, Publisher, Genres, Annotation), with an optional availability filter.
    /// </summary>
    /// <param name="searchTerm">
    /// The value to search for in the searchable fields.
    /// </param>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status.
    /// If <c>null</c>, no availability filter is applied.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a read-only collection of matching books.
    /// </returns>
    public async Task<IReadOnlyCollection<Book>> GetBooksByPartialMatchAsync(string searchTerm, bool? isAvailable)
    {
        var regex = BuildCaseInsensitiveRegex(searchTerm);

        var filter = Builders<Book>.Filter.Or(
            Builders<Book>.Filter.Regex(b => b.Title, regex),
            Builders<Book>.Filter.Regex(b => b.Language, regex),
            Builders<Book>.Filter.Regex(b => b.Annotation, regex),
            Builders<Book>.Filter.Regex(b => b.Publisher, regex),

            Builders<Book>.Filter.Regex("Authors", regex),
            Builders<Book>.Filter.Regex("Genres", regex)
            );

        if(isAvailable.HasValue)
        {
            var availabilityFilter =
            Builders<Book>.Filter.Eq(b => b.IsAvailable, isAvailable.Value);
            filter = Builders<Book>.Filter.And(filter, availabilityFilter);
        }

        return await _booksCollection.Find(filter).ToListAsync();
    }

    #region Helpers

    /// <summary>
    /// Normalizes the provided string by trimming any leading or trailing whitespace 
    /// and converting it to lowercase using the invariant culture.
    /// </summary>
    /// <param name="input">
    /// The string to be normalized. Must not be <c>null</c>.
    /// </param>
    /// <returns>
    /// A trimmed, lowercase version of the input string.
    /// </returns>
    /// <remarks>
    /// This method is intendent for internal use to ensure consistent, 
    /// case-insensitive string comparisons during search operations.
    /// </remarks>
    private static string NormalizeString(string input) => input.Trim().ToLower();

    /// <summary>
    /// Creates a case-insensitive MongoDB <see cref="BsonRegularExpression"/>
    /// </summary>
    /// <param name="input">
    /// The input string used to build the regular expression.
    /// </param>
    /// <returns>
    /// A case-insensitive <see cref="BsonRegularExpression"/> for MongoDB queries.
    /// </returns>
    private static BsonRegularExpression BuildCaseInsensitiveRegex(string input)
    {
        var normalized = NormalizeString(input);
        return new BsonRegularExpression(normalized, "i");
    }
    #endregion Helpers
}
