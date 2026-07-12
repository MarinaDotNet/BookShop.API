using AutoMapper;
using BookShop.API.DTOs.Catalog;
using BookShop.API.DTOs.Shared;
using BookShop.API.Exceptions;
using BookShop.API.Helpers;
using BookShop.API.Infrastructure.Persistence;
using BookShop.API.Models.Catalog;
using BookShop.API.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookShop.API.Services;

/// <summary>
/// Provides operations for retrieving book data from a repository.
/// </summary>
/// <remarks>This service acts as an abstraction over the underlying book repository, allowing consumers to
/// retrieve book information asynchronously. Thread safety depends on the implementation of the provided
/// repository.</remarks>
/// <param name="bookRepository">The repository used to access book data. Cannot be null.</param>
/// <param name="mapper">The mapper used to map entities to DTOs. Cannot be null.</param>
public class BookService(IBookRepository bookRepository, IMapper mapper) : IBookService
{
    private readonly IBookRepository _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    #region Getters

    /// <summary>
    /// Asynchronously retrieves all books from the data source,
    /// with an optional filter for availability.
    /// </summary>
    /// <param name="isAvailable">
    /// Optional availability filter.
    /// If null, all books are retrieved.
    /// If true or false, only books matching the specified availability are returned.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A paginated result containing book DTOs and pagination metadata.
    /// </returns>
    public async Task<PageResultDto<BookDto>> GetAllBooksAsync(bool? isAvailable, PaginationQueryDto pagination, CancellationToken cancellationToken)
    {
        PageResultDto<Book> query = await _bookRepository.GetAllBooksAsync(isAvailable, pagination, cancellationToken);

        return PaginationHelper.MapPageResult<Book, BookDto>(_mapper, query);
    }


    /// <summary>
    /// Asynchronously retrieves a book by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to retrieve. Cannot be null or empty.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="BookDto"/> representing
    /// the requested book.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown if a book with the specified <paramref name="id"/> does not exist.
    /// </exception>
    public async Task<BookDto> GetBookByIdAsync(string id, CancellationToken cancellationToken)
    {
        ValidateObjectId(id);

        var book = await _bookRepository.GetBookByIdAsync(id, cancellationToken);

        return book is null 
            ? throw new NotFoundException($"Book with the provided ID '{id}' was not found.") 
            : _mapper.Map<BookDto>(book);
    }

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
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a paginated read-only collection of <see cref="BookDto"/> objects that match the specified search criteria.
    /// If no books are found matching the criteria, an empty collection is returned.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <paramref name="request"/>  is null.
    /// </exception>
    public async Task<PageResultDto<BookDto>> GetBooksByExactMatchAsync(
        BookSearchRequestDto request, 
        PaginationQueryDto pagination, 
        CancellationToken cancellationToken)
    {
        if(request is null)
        {
            throw new ValidationException("Search term cannot be null.");
        }
        return await GetBooksByExactMatchAsync(request, request.IsAvailable, pagination, cancellationToken);
    }
        

    /// <summary>
    /// Asynchronously retrieves available books that exactly match the specified search term.
    /// </summary>
    /// <param name="request">
    /// The search request containing the search term.
    /// </param>
    ///  <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a paginated read-only collection of <see cref="BookDto"/> objects that match the specified search criteria.
    /// If no books are found matching the criteria, an empty collection is returned.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <paramref name="request"/>  is null.
    /// </exception>
    public async Task<PageResultDto<BookDto>> GetAvailableBooksByExactMatchAsync(
        BookSearchRequestDto request, 
        PaginationQueryDto pagination, 
        CancellationToken cancellationToken)
    {
        if(request is null)
        {
            throw new ValidationException("Search term cannot be null.");
        }
        return await GetBooksByExactMatchAsync(request, true, pagination, cancellationToken);
    }

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
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a paginated read-only collection of <see cref="BookDto"/> 
    /// objects that match the specified search criteria.
    /// If no books are found matching the criteria, an empty collection is returned.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <paramref name="request"/>  is null.
    /// </exception>
    public async Task<PageResultDto<BookDto>> GetBooksByPartialMatchAsync(
        BookSearchRequestDto request, 
        PaginationQueryDto pagination, 
        CancellationToken cancellationToken)
    {
        if(request is null)
        {
            throw new ValidationException("Search term cannot be null.");
        }
        return await GetBooksByPartialMatchAsync(request, request.IsAvailable, pagination, cancellationToken);
    }
        

    /// <summary>
    /// Asynchronously retrieves available books that partially match the specified search term.
    /// </summary>
    /// <param name="request">
    /// The search request containing the search term.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a paginated read-only collection of <see cref="BookDto"/> objects that match the specified search criteria.
    /// If no books are found matching the criteria, an empty collection is returned.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <paramref name="request"/>  is null.
    /// </exception>
    public async Task<PageResultDto<BookDto>> GetAvailableBooksByPartialMatchAsync(
        BookSearchRequestDto request, 
        PaginationQueryDto pagination, 
        CancellationToken cancellationToken)
    {
         if(request is null)
        {
            throw new ValidationException("Search term cannot be null.");
        }
        return await GetBooksByPartialMatchAsync(request, true, pagination, cancellationToken);
    }
        

    /// <summary>
    /// Asynchronously checks if a book with the specified identifier exists
    /// in the data source.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to check. Cannot be <c>null</c> or empty.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> representing the asynchronous operation.
    /// The task result is <c>true</c> if the book exists; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when the provided identifier is null, empty, or not a valid ObjectId.
    /// </exception>
    public async Task<bool> IsBookExistsAsync(string id, CancellationToken cancellationToken)
    {
        ValidateObjectId(id);

        var book = await _bookRepository.GetBookByIdAsync(id, cancellationToken);

        return book is not null;
    }

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
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="BookDto"/>
    /// representing the top cheapest books that match the specified criteria. If no books are found matching the criteria, an empty
    /// collection is returned.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="count"/> is not a positive integer.
    /// </exception>
    public async Task<IReadOnlyCollection<BookDto>> GetTopCheapestBooksAsync(int count, bool? isAvailable, CancellationToken cancellationToken)
    {
        if(count <= 0)
        {
            throw new ValidationException("Count must be a positive integer.");
        }

        var books = await _bookRepository.GetTopCheapestBooksAsync(count, isAvailable, cancellationToken);
        
        return _mapper.Map<IReadOnlyCollection<BookDto>>(books);
    }

    /// <summary>
    /// Asynchronously retrieves the top expensive books from the data source, with an optional filter for availability.
    /// </summary>
    /// <param name="count">
    /// The maximum number of expensive books to retrieve. Must be a positive integer.
    /// </param>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status. If null then no availability filter is applied.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="BookDto"/>
    /// objects representing the top expensive books that match the specified criteria. If no books are found matching the criteria,
    /// an empty collection is returned. 
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="count" /> is not a positive integer.
    /// </exception>
    public async Task<IReadOnlyCollection<BookDto>> GetTopExpensiveBooksAsync(int count, bool? isAvailable, CancellationToken cancellationToken)
    {
        if(count <= 0)
        {
            throw new ValidationException("Count must be a positive integer.");
        }

        var books = await _bookRepository.GetTopExpensiveBooksAsync(count, isAvailable, cancellationToken);
        return _mapper.Map<IReadOnlyCollection<BookDto>>(books);
    }
    #endregion Getters

    #region Setters

    /// <summary>
    /// Asynchronously adds a new book to the data source.
    /// </summary>
    /// <param name="bookDto">
    /// A <see cref="BookDto"/> object containing the details of the book to be added.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task{BookDto}"/> representing the asynchronous operation.
    /// The task result contains the added <see cref="BookDto"/> object, including the generated Id.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown if <paramref name="bookDto"/> is invalid according to business rules.
    /// </exception>
    public async Task<BookCreateDto> CreateBookAsync(BookCreateDto bookDto, CancellationToken cancellationToken)
    {
        var book = _mapper.Map<Book>(bookDto);
        var addedBook = await _bookRepository.AddBookAsync(book, cancellationToken);

        return _mapper.Map<BookCreateDto>(addedBook);
    }

    /// <summary>
    /// Asynchronously deletes the <see cref="Book"/> with the specified identifier
    /// from the data source.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to be deleted. Cannot be <c>null</c> or empty.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task{BookDto}"/> representing the asynchronous operation.
    /// The task result contains the deleted <see cref="BookDto"/>.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when the provided identifier is null, empty, or not a valid ObjectId.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown when a book with the specified identifier does not exist.
    /// </exception>
    public async Task<BookDto> DeleteBookByIdAsync(string id, CancellationToken cancellationToken)
    {
        ValidateObjectId(id);

        if(!await IsBookExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Book with ID '{id}' not found.");
        }

        return _mapper.Map<BookDto>(await _bookRepository.DeleteBookByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Asynchronously updates an existing book in the data source.
    /// All fields in <paramref name="bookDto"/> are applied to the existing record.
    /// </summary>
    /// <param name="bookDto">
    /// A <see cref="BookUpdateDto"/> containing the updated data. Cannot be <c>null</c> and must include a valid book ID.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// The task result contains the updated <see cref="BookDto"/> object.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when a book with the specified ID does not exist.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the update operation fails.
    /// </exception>
    public async Task<BookDto> UpdateBookAsync(BookUpdateDto bookDto, CancellationToken cancellationToken)
    {
        if (!await IsBookExistsAsync(bookDto.Id, cancellationToken))
        {
            throw new NotFoundException($"Book with ID '{bookDto.Id}' not found.");
        }

        var updatedBook = await _bookRepository.UpdateBookAsync(_mapper.Map<Book>(bookDto), cancellationToken) ??
            throw new InvalidOperationException("Book update failed.");

        return _mapper.Map<BookDto>(updatedBook);
    }

    /// <summary>
    /// Partially updates an existing book in the data source using PATCH semantics.
    /// Only fields provided and considered valid in <paramref name="bookDto"/> are applied; other fields remain unchanged.
    /// The book must exist before any update is performed.
    /// </summary>
    /// <param name="bookDto">
    /// A <see cref="BookUpdatePartlyDto"/> containing the fields to update. Cannot be <c>null</c> and must include a valid book ID.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// The task result contains the updated <see cref="BookDto"/> object.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="bookDto"/> is <c>null</c> or contains no valid fields to update, or
    /// when the book identifier is invalid.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown when a book with the specified ID does not exist in the data source.
    /// </exception>
    /// <remarks>
    /// Each field is only updated if it passes the corresponding validation defined in the service.
    /// For example, string fields must not be null or whitespace, numeric fields must be positive, and collections must not be empty.
    /// </remarks>
    public async Task<BookDto> UpdateBookPartlyAsync(BookUpdatePartlyDto bookDto, CancellationToken cancellationToken)
    {
        if (!await IsBookExistsAsync(bookDto.Id, cancellationToken))
        {
            throw new NotFoundException($"Book with ID '{bookDto.Id}' not found.");
        }

        var updates = new List<UpdateDefinition<Book>>();

        updates.AddIfNotNull(bookDto.Title, b => b.Title, v => !string.IsNullOrWhiteSpace(v));
        updates.AddIfNotNull(bookDto.Publisher, b => b.Publisher, v => !string.IsNullOrWhiteSpace(v));
        updates.AddIfNotNull(bookDto.Language, b => b.Language, v => !string.IsNullOrWhiteSpace(v));
        updates.AddIfNotNull(bookDto.Annotation, b => b.Annotation, v => !string.IsNullOrWhiteSpace(v));
        updates.AddIfNotNull(bookDto.Authors, b => b.Authors, v => !IsNullOrEmptyStringCollection(v));
        updates.AddIfNotNull(bookDto.Genres, b => b.Genres, v => !IsNullOrEmptyStringCollection(v));
        updates.AddIfNotNull(bookDto.Price, b => b.Price, v => v > 0);
        updates.AddIfNotNull(bookDto.Pages, b => b.Pages, v => v > 0);
        updates.AddIfNotNull(bookDto.Link, b => b.Link, v => !string.IsNullOrWhiteSpace(v.ToString()) && Uri.IsWellFormedUriString(v.ToString(), UriKind.Absolute));
        updates.AddIfNotNull(bookDto.IsAvailable, b => b.IsAvailable);

        if (updates.Count == 0)
        {
            throw new ValidationException("No valid fields provided for update.");
        }

        var updatedBook = await _bookRepository.UpdateBookPartlyAsync(updates, bookDto.Id, cancellationToken);

        return updatedBook is not null 
            ? _mapper.Map<BookDto>(updatedBook) 
            : throw new NotFoundException($"Book with ID '{bookDto.Id}' not found.");
            
    }

    #endregion Setters

    #region Helpers

    /// <summary>
    /// Validates that the specified object ID is not null, not empty, and is in a valid format.
    /// </summary>
    /// <param name="id">The object ID to validate. Must be a non-empty, non-whitespace string in a valid ObjectId format.</param>
    /// <exception cref="ValidationException">Thrown if the object ID is null, empty, or not in a valid ObjectId format.</exception>
    private static void ValidateObjectId(string id)
    {
        if(id is null || string.IsNullOrWhiteSpace(id))
        {
            throw new ValidationException("Book ID cannot be empty or null.");
        }

        if(!ObjectId.TryParse(id, out _))
        {
            throw new ValidationException("Invalid Book ID format.");
        }
    }

    /// <summary>
    /// Determines whether a collection of strings is null, empty, or contains any null, empty, or whitespace elements.
    /// </summary>
    /// <param name="collection">
    /// The collection of strings to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if <paramref name="collection"/> is <c>null</c>, empty, or contains any null, empty, or whitespace elements; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsNullOrEmptyStringCollection(List<string>? collection)
    {
        return collection is null || collection.Count == 0 || collection.Any(string.IsNullOrWhiteSpace);
    }

    /// <summary>
    /// Asynchronously retrieves books that exactly match the specified search term with an optional availability filter.
    /// </summary>
    /// <param name="request">
    /// The search request containing the search term and optional availability filter.
    /// </param>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status.
    /// If null then no availability filter is applied.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a paginated read-only collection of <see cref="BookDto"/> 
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <paramref name="request"/> contains an invalid search term.
    /// </exception>
    private async Task<PageResultDto<BookDto>> GetBooksByExactMatchAsync(BookSearchRequestDto request, bool? isAvailable, PaginationQueryDto pagination, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            throw new ValidationException("Search term cannot be null or empty.");
        }

        var query = await _bookRepository.GetBooksByExactMatchAsync(request.SearchTerm, isAvailable, pagination, cancellationToken);

        return PaginationHelper.MapPageResult<Book, BookDto>(_mapper, query);
    }

    /// <summary>
    /// Asynchronously retrieves books that partially match the specified search term with an optional availability filter.
    /// </summary>
    /// <param name="request">
    /// The search request containing the search term and optional availability filter.
    /// </param>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status.
    /// If null then no availability filter is applied.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a paginated read-only collection of <see cref="BookDto"/>. 
    /// </returns>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <paramref name="request"/> contains an invalid search term.
    /// </exception>
    private async Task<PageResultDto<BookDto>> GetBooksByPartialMatchAsync(BookSearchRequestDto request, bool? isAvailable, PaginationQueryDto pagination, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            throw new ValidationException("Search term cannot be null or empty.");
        }

        var query = await _bookRepository.GetBooksByPartialMatchAsync(request.SearchTerm, isAvailable, pagination, cancellationToken);

        return PaginationHelper.MapPageResult<Book, BookDto>(_mapper, query);
    }

    /// <summary>
    /// Asynchronously retrieves books that match the specified search criteria and sorting options.
    /// </summary>
    /// <param name="query">
    /// The query object containing search criteria, and sorting options.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a paginated read-only collection of
    /// <see cref="BookDto"/> objects that match the specified search criteria and sorting options. 
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="query"/> or <paramref name="pagination"/> is null.
    /// </exception>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <see cref="PaginationQueryDto.PageNumber"/> is less than 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> is less than 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> exceeds <see cref="PaginationQueryDto.MaxPageSize"/>.
    /// - <see cref="BookSearchRequestDto.SearchTerm"/> is null or empty.
    /// </exception>   
    public async Task<PageResultDto<BookDto>> GetSortedAndFilteredBooksAsync(BookQueryDto query, PaginationQueryDto pagination, CancellationToken cancellationToken)
    {
        var result = await _bookRepository.GetSortedAndFilteredBooksAsync(query, pagination, cancellationToken);

        return PaginationHelper.MapPageResult<Book, BookDto>(_mapper, result);
    }

    /// <summary>
    /// Asynchronously retrieves available books that match the specified search criteria and sorting options.
    /// </summary>
    /// <param name="query">
    /// The query object containing search criteria, and sorting options.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a paginated read-only collection of available
    /// <see cref="BookDto"/> objects that match the specified search criteria and sorting options. 
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="query"/> or <paramref name="pagination"/> is null.
    /// </exception>
    /// <exception cref="ValidationException">
    /// Thrown when:
    /// - <see cref="PaginationQueryDto.PageNumber"/> is less than 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> is less than 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> exceeds <see cref="PaginationQueryDto.MaxPageSize"/>.
    /// - <see cref="BookSearchRequestDto.SearchTerm"/> is null or empty.
    /// </exception>   
    public async Task<PageResultDto<BookDto>> GetSortedAndFilteredAvailableBooksAsync(BookQueryDto query, PaginationQueryDto pagination, CancellationToken cancellationToken)
    {
        var result = await _bookRepository.GetSortedAndFilteredBooksAsync(query with { IsAvailable = true }, pagination, cancellationToken);

        return PaginationHelper.MapPageResult<Book, BookDto>(_mapper, result);
    }
    #endregion Helpers
}