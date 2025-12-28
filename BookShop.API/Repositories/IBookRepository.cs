using BookShop.API.Models;
using MongoDB.Driver;

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

    /// <summary>
    /// Asynchronously checks if a book with the specified ID exists in the collection.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to check. Cannot be <c>null</c> or empty.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> representing the asynchronous operation.
    /// The task result is <c>true</c> if the book exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> GetBookByIdAnyAsync(string id);

    /// <summary>
    /// Asynchronously adds a new <see cref="Book"/> to the data source.
    /// </summary>
    /// <param name="book">
    /// The <see cref="Book"/> object containing the details of the book to be added.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Book}"/> representing the asynchronous operation.
    /// The task result contains the added <see cref="Book"/> object, including the generated Id.
    /// </returns>
    Task<Book> AddBookAsync(Book book);

    /// <summary>
    /// Asynchronously deletes the <see cref="Book"/> with the specified identifier from the data source.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to be deleted. Cannot be <c>null</c> or empty.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Book}"/> representing the asynchronous operation.
    /// The task result contains the deleted <see cref="Book"/> object.
    /// </returns>
    /// <remarks>
    /// If the book with the specified ID does not exist, a <see cref="NotFoundException"/>
    /// is typically thrown by the service layer.
    /// </remarks>
    Task<Book> DeleteBookByIdAsync(string id);

    /// <summary>
    /// Asynchronously updates a full <see cref="Book"/> entity in the data source.
    /// </summary>
    /// <param name="book">The <see cref="Book"/> object containing updated data. Must have a valid ID.</param>
    /// <returns>
    /// A <see cref="Task{Book}"/> representing the asynchronous operation.
    /// The task result contains the updated <see cref="Book"/> entity.
    /// </returns>
    Task<Book> UpdateBookAsync(Book book);

    /// <summary>
    /// Asynchronously applies partial updates to a <see cref="Book"/> using MongoDB <see cref="UpdateDefinition{Book}"/>.
    /// </summary>
    /// <param name="updates">A list of update definitions specifying the fields to update.</param>
    /// <param name="id">The unique identifier of the book to update.</param>
    /// <returns>
    /// A <see cref="Task{Book?}"/> representing the asynchronous operation.
    /// The task result contains the updated <see cref="Book"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<Book?> UpdateBookPartlyAsync(List<UpdateDefinition<Book>> updates, string id);
}
