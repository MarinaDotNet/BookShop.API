using BookShop.API.DTOs.Catalog;
using BookShop.API.Models.Catalog;

namespace BookShop.API.Services;

/// <summary>
/// Provides operation for managing books in the catalog, including retrieval, creation, updating, and deletion of book records.
/// </summary>
public interface IBookService
{
    #region Getters

    /// <summary>
    /// Asynchronously retrieves all books from the catalog. This method returns a collection of <see cref="BookDto"/> objects representing the
    /// books available in the system. 
    /// The returned collection may be empty if no books are found. 
    /// </summary>
    /// <param name="isAvailable">
    /// An optional filter parameter that, if specified, will return only books that are currently available (true) or 
    /// unavailable (false). If null, the method returns all books regardless of their availability status.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="BookDto"/> 
    /// </returns>
    Task<IReadOnlyCollection<BookDto>> GetAllBooksAsync(bool? isAvailable);

    /// <summary>
    /// Asynchronously retrieves a book by its unique identifier. This method returns a <see cref="BookDto"/> object representing the book with the 
    /// specified ID. If no book is found with the given ID, the method returns null.
    /// The ID parameter is expected to be a non-empty string that uniquely identifies a book in the catalog. 
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to retrieve. Must be a non empty string.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the <see cref="BookDto"/> object representing the book with the specified ID, or null if no book is found.
    /// </returns>
    Task<BookDto> GetBookByIdAsync(string id);

    /// <summary>
    /// Asynchronously retrieves books that exactly match the specified search term with an 
    /// optional availability filter.
    /// </summary>
    /// <param name="request">
    /// The search request containing the search term and optional availability filter.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a read-only collection of <see cref="BookDto"/>
    /// objects that match the specified search criteria.
    /// </returns>
    Task<IReadOnlyCollection<BookDto>> GetBooksByExactMatchAsync(BookSearchRequestDto request);

    /// <summary>
    /// Asynchronously retrieves books that partially match the specified search term
    /// with an optional availability filter.
    /// </summary>
    /// <param name="request">
    /// The search request containing the search term and an optional availability filter.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a read-only 
    /// collection of <see cref="BookDto"/> objects that match the specified search criteria.
    /// </returns>
    Task<IReadOnlyCollection<BookDto>> GetBooksByPartialMatchAsync(BookSearchRequestDto request);

    /// <summary>
    /// Asynchronously checks if a book with the specified identifier exists
    /// in the data source.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to check. Cannot be <c>null</c> or empty.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> representing the asynchronous operation.
    /// The task result is <c>true</c> if the book exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsBookExistsAsync(string id);

    /// <summary>
    /// Asynchronously retrieves the top cheapest books from the data source, with an optional filter for availability.
    /// </summary>
    /// <param name="count">
    /// The number of cheapest books to retrieve. Must be a positive integer.
    /// </param>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status.
    /// If null then no availability filter is applied.
    /// </param>
    /// <returns>
    /// A <see cref="BookDto"/> representing the asynchronous operation.
    /// The task result contains a read-only collection of <see cref="BookDto"/> objects representing the top cheapest books 
    /// that match the specified criteria.
    /// </returns>
    Task<IReadOnlyCollection<BookDto>> GetTopCheapestBooksAsync(int count, bool? isAvailable);

    #endregion Getters

    #region Setters

    /// <summary>
    /// Asynchronously adds a new book to the data source.
    /// </summary>
    /// <param name="bookDto">
    /// A <see cref="BookDto"/> object containing the details of the book to be added.
    /// </param>
    /// <returns>
    /// A <see cref="Task{BookDto}"/> representing the asynchronous operation.
    /// The task result contains the added <see cref="BookDto"/> object, including the generated Id.
    /// </returns>
    Task<BookDto> CreateBookAsync(BookDto bookDto);

    /// <summary>
    /// Asynchronously deletes the <see cref="Book"/> with the specified identifier
    /// from the data source.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to be deleted. Cannot be <c>null</c> or empty.
    /// </param>
    /// <returns>
    /// A <see cref="Task{BookDto}"/> representing the asynchronous operation.
    /// The task result contains the deleted <see cref="BookDto"/>.
    /// </returns>
    Task<BookDto> DeleteBookByIdAsync(string id);

    /// <summary>
    /// Asynchronously updates an existing book in the data source.
    /// All fields in <paramref name="bookDto"/> are applied to the existing record.
    /// </summary>
    /// <param name="bookDto">
    /// A <see cref="BookDto"/> containing the updated data. Cannot be <c>null</c> and must include a valid book ID.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// The task result contains the updated <see cref="BookDto"/> object.
    /// </returns>
    Task<BookDto> UpdateBookAsync(BookDto bookDto);

    /// <summary>
    /// Partially updates an existing book in the data source using PATCH semantics.
    /// Only fields provided and considered valid in <paramref name="bookDto"/> are applied; other fields remain unchanged.
    /// </summary>
    /// <param name="bookDto">
    /// A <see cref="BookUpdateDto"/> containing the fields to update. Cannot be <c>null</c> and must include a valid book ID.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// The task result contains the updated <see cref="BookDto"/> object.
    /// </returns>
    Task<BookDto> UpdateBookPartlyAsync(BookUpdateDto bookDto);

    #endregion Setters
}