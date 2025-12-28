using BookShop.API.Models;

namespace BookShop.API.Repositories;

/// <summary>
/// Represents a repository that provides asynchronous access to book data.
/// </summary>
public interface IBookRepository
{
    /// <summary>
    /// Asynchronously retrieves all books from the data source, with optional filter for availability.
    /// </summary>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status. 
    /// If provided, only books match the specified availability status will be returned. 
    /// If null, no avialability filter is applied.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of books 
    /// that match the availability filter, or all books if filter not applied. 
    /// The collection is empty if no books are found.
    /// </returns>
    Task<IReadOnlyCollection<Book>> GetAllBooksAsync(bool? isAvailable);

    /// <summary>
    /// Asynchronously retrieves a book with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the book to retrieve. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Book"/> with the
    /// specified identifier, or <see langword="null"/> if no matching book is found.</returns>
    Task<Book> GetBookByIdAsync(string id);

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
    Task<IReadOnlyCollection<Book>> GetBooksByExactMatchAsync(string searchTerm, bool? isAvailable);

    /// <summary>
    /// Asychronously retrieves <see cref="Book"/> documents from MongoDB that contains the specified
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
    Task<IReadOnlyCollection<Book>> GetBooksByPartialMatchAsync(string searchTerm, bool? isAvailable);
}
