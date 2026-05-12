using BookShop.API.DTOs.Catalog;
using BookShop.API.Models.Catalog;
using BookShop.API.Exceptions;
using BookShop.API.DTOs.Shared;

namespace BookShop.API.Services;

/// <summary>
/// Provides operations for managing books in the catalog, including retrieval, creation, updating, and deletion of book records.
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
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a paginated read-only collection of 
    /// <see cref="BookDto"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the pagination object is null.
    /// </exception> 
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <see cref="PaginationQueryDto.PageNumber"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> exceeds <see cref="PaginationQueryDto.MaxPageSize"/>.
    /// </exception>
    Task<PageResultDto<BookDto>> GetAllBooksAsync(bool? isAvailable, PaginationQueryDto pagination);

    /// <summary>
    /// Asynchronously retrieves a book by its unique identifier. This method returns a <see cref="BookDto"/> object representing the book with the 
    /// specified ID.
    /// The ID parameter is expected to be a non-empty string that uniquely identifies a book in the catalog. 
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to retrieve. Must be a non-empty string.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the <see cref="BookDto"/> object 
    /// representing the book with the specified ID.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided identifier is null, empty, or not a valid ObjectId.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown when a book with the specified identifier is not found.
    /// </exception>  
    Task<BookDto> GetBookByIdAsync(string id);

    /// <summary>
    /// Asynchronously retrieves books that exactly match the specified search term with an 
    /// optional availability filter.
    /// </summary>
    /// <param name="request">
    /// The search request containing the search term and optional availability filter.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a paginated read-only collection of <see cref="BookDto"/>
    /// objects that match the specified search criteria.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <see cref="PaginationQueryDto.PageNumber"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> exceeds <see cref="PaginationQueryDto.MaxPageSize"/>.
    /// - <paramref name="request"/>  is null or contains an invalid search term.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the pagination object is null.
    /// </exception>

    Task<PageResultDto<BookDto>> GetBooksByExactMatchAsync(BookSearchRequestDto request, PaginationQueryDto pagination);

    /// <summary>
    /// Asynchronously retrieves available books that exactly match the specified search term.
    /// </summary>
    /// <param name="request">
    /// The search request containing the search term.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a read-only collection of <see cref="BookDto"/>
    /// objects that match the specified search criteria.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <see cref="PaginationQueryDto.PageNumber"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> exceeds <see cref="PaginationQueryDto.MaxPageSize"/>.
    /// - <paramref name="request"/>  is null or contains an invalid search term.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the pagination object is null.
    /// </exception>

    Task<PageResultDto<BookDto>> GetAvailableBooksByExactMatchAsync(BookSearchRequestDto request, PaginationQueryDto pagination);

    /// <summary>
    /// Asynchronously retrieves books that partially match the specified search term
    /// with an optional availability filter.
    /// </summary>
    /// <param name="request">
    /// The search request containing the search term and an optional availability filter.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param> 
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a paginated read-only 
    /// collection of <see cref="BookDto"/> objects that match the specified search criteria.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <see cref="PaginationQueryDto.PageNumber"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> exceeds <see cref="PaginationQueryDto.MaxPageSize"/>.
    /// - <see cref="BookSearchRequestDto.SearchTerm"/> is null or empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the pagination object is null.
    /// </exception>
    Task<PageResultDto<BookDto>> GetBooksByPartialMatchAsync(BookSearchRequestDto request, PaginationQueryDto pagination);

    /// <summary>
    /// Asynchronously retrieves available books that partially match the specified search term.
    /// </summary>
    /// <param name="request">
    /// The search request containing the search term.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param> 
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a paginated read-only 
    /// collection of <see cref="BookDto"/> objects that partly match the specified search criteria.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <see cref="PaginationQueryDto.PageNumber"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> exceeds <see cref="PaginationQueryDto.MaxPageSize"/>.
    /// - <see cref="BookSearchRequestDto.SearchTerm"/> is null or empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the pagination object is null.
    /// </exception>
    Task<PageResultDto<BookDto>> GetAvailableBooksByPartialMatchAsync(BookSearchRequestDto request, PaginationQueryDto pagination);

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
    /// <exception cref="ValidationException">
    /// Thrown when the provided identifier is null, empty, or not a valid ObjectId
    /// </exception>
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
    /// A task that represents a read-only collection of <see cref="BookDto"/> objects representing the top cheapest books 
    /// that match the specified criteria.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="count" />is not a positive integer.
    /// </exception> 
    Task<IReadOnlyCollection<BookDto>> GetTopCheapestBooksAsync(int count, bool? isAvailable);

    /// <summary>
    /// Asynchronously retrieves the top expensive books from the data source, with an optional filter for availability.
    /// </summary>
    /// <param name="count">
    /// The maximum number of expensive books to retrieve. Must be a positive integer.
    /// </param>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status.
    /// If null then no availability filter is applied.
    /// </param>
    /// <returns>
    /// A task that represents a read-only collection of <see cref="BookDto"/> objects representing the top expensive books
    /// that match the specified criteria.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="count" /> is not a positive integer.
    /// </exception>  
    Task<IReadOnlyCollection<BookDto>> GetTopExpensiveBooksAsync(int count, bool? isAvailable);

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
    /// <exception cref="ValidationException">
    /// Thrown when the provided identifier is null, empty, or not a valid ObjectId
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown when a book with the specified identifier does not exist in the data source.
    /// </exception> 
    Task<BookDto> DeleteBookByIdAsync(string id);

    /// <summary>
    /// Asynchronously updates an existing book in the data source.
    /// All fields in <paramref name="bookDto"/> are applied to the existing record.
    /// </summary>
    /// <param name="bookDto">
    /// A <see cref="BookDto"/> containing the updated data. Cannot be <c>null</c> and must include a valid book ID.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the updated <see cref="BookDto"/> object. 
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="bookDto"/> is <c>null</c> or contains invalid data.
    /// </exception>
    ///  <exception cref="NotFoundException">
    /// Thrown when a book with the specified ID does not exist in the data source.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the update operation fails due to an unexpected error.
    /// </exception>
    Task<BookDto> UpdateBookAsync(BookDto bookDto);

    /// <summary>
    /// Partially updates an existing book in the data source using PATCH semantics.
    /// Only fields provided and considered valid in <paramref name="bookDto"/> are applied; other fields remain unchanged.
    /// </summary>
    /// <param name="bookDto">
    /// A <see cref="BookUpdateDto"/> containing the fields to update. Cannot be <c>null</c> and must include a valid book ID.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the updated <see cref="BookDto"/> object. 
    /// </returns>
    Task<BookDto> UpdateBookPartlyAsync(BookUpdateDto bookDto);

    #endregion Setters
}