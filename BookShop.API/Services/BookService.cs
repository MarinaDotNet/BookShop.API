using AutoMapper;
using BookShop.API.Exceptions;
using BookShop.API.Infrastructure;
using BookShop.API.Models;
using BookShop.API.Repositories;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace BookShop.API.Services;

/// <summary>
/// Provides operations for retrieving book data from a repository.
/// </summary>
/// <remarks>This service acts as an abstraction over the underlying book repository, allowing consumers to
/// retrieve book information asynchronously. Thread safety depends on the implementation of the provided
/// repository.</remarks>
/// <param name="bookRepository">The repository used to access book data. Cannot be null.</param>
/// <param name="mapper">The mapper used to map entities to DTOs. Cannot be null.</param>
public class BookService(IBookRepository bookRepository, IMapper mapper)
{
    private readonly IBookRepository _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
    private readonly IMapper _mapper = mapper;

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
    /// <returns>
    /// A read-only collection of <see cref="BookDto"/> objects
    /// that match the specified criteria.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no books are found for the given filter.
    /// </exception>
    public async Task <IReadOnlyCollection<BookDto>> GetAllBooksAsync(bool? isAvailable)
    {
        var books = await _bookRepository.GetAllBooksAsync(isAvailable);

        if (!books.Any())
        {
            throw new NotFoundException("No books found.");
        }

        return _mapper.Map<IReadOnlyCollection<BookDto>>(books);
    }


    /// <summary>
    /// Asynchronously retrieves a book by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the book to retrieve. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="BookDto"/> representing
    /// the requested book.</returns>
    /// <exception cref="NotFoundException">Thrown if a book with the specified <paramref name="id"/> does not exist.</exception>
    public async Task<BookDto> GetBookByIdAsync(string id)
    {
        ValidateObjectId(id);

        var book = await _bookRepository.GetBookByIdAsync(id);

        return book is null 
            ? throw new NotFoundException($"Book with ID '{id}' not found.") 
            : _mapper.Map<BookDto>(book);
    }

    /// <summary>
    /// Asynchronously retrieves books that exactly match the specified search term with an 
    /// optional availability filter.
    /// </summary>
    /// <param name="request">
    /// The search request containing the search term and optional availability filter.
    /// </param>
    /// A task that represents the asynchronous operation.
    /// The task result contains a read-only collection of <see cref="BookDto"/>
    /// objects that match the specified search criteria.
    /// <exception cref="ValidationException">
    /// Thrown if the <paramref name="searchTerm"/> is null or empty.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown if no books are found matching the search criteria.
    /// </exception>
    public async Task<IReadOnlyCollection<BookDto>> GetBooksByExactMatchAsync(BookSearchRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            throw new ValidationException("Search term cannot be null or empty.");
        }

        var books = await _bookRepository.GetBooksByExactMatchAsync(request.SearchTerm, request.IsAvailable);

        if (books.Count == 0)
        {
            throw new NotFoundException("No books found matching the search criteria.");
        }

        return _mapper.Map<IReadOnlyCollection<BookDto>>(books);
    }

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
    /// <exception cref="ValidationException">
    /// Thrown if <see cref="BookSearchRequestDto.SearchTerm"/> is null or empty.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown if no books are found matching the search criteria.
    /// </exception>
    public async Task<IReadOnlyCollection<BookDto>> GetBooksByPartialMatchAsync(BookSearchRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            throw new ValidationException("Search term cannot be null or empty.");
        }

        var books = await _bookRepository.GetBooksByPartialMatchAsync(request.SearchTerm, request.IsAvailable);

        if (books.Count == 0)
        {
            throw new NotFoundException("No books found matching the search criteria.");
        }

        return _mapper.Map<IReadOnlyCollection<BookDto>>(books);
    }

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
    /// Thrown when the provided identifier is null, empty, or not a valid ObjectId.
    /// </exception>
    public async Task<bool> IsBookExistsAsync(string id)
    {
        ValidateObjectId(id);

        var book = await _bookRepository.GetBookByIdAsync(id);

        return book is not null;
    }
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
    /// <exception cref="ValidationException">
    /// Thrown if <paramref name="bookDto"/> is invalid according to business rules.
    /// </exception>
    public async Task<BookDto> CreateBookAsync(BookDto bookDto)
    {
        ValidateBookDto(bookDto);

        var book = _mapper.Map<Book>(bookDto);
        var addedBook = await _bookRepository.AddBookAsync(book);

        return _mapper.Map<BookDto>(addedBook);
    }

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
    /// Thrown when the provided identifier is null, empty, or not a valid ObjectId.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown when a book with the specified identifier does not exist.
    /// </exception>
    public async Task<BookDto> DeleteBookByIdAsync(string id)
    {
        ValidateObjectId(id);

        if(!await IsBookExistsAsync(id))
        {
            throw new NotFoundException($"Book with ID '{id}' not found.");
        }

        return _mapper.Map<BookDto>(await _bookRepository.DeleteBookByIdAsync(id));
    }

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
    /// <exception cref="NotFoundException">
    /// Thrown when a book with the specified ID does not exist.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the update operation fails.
    /// </exception>
    public async Task<BookDto> UpdateBookAsync(BookDto bookDto)
    {
        ValidateBookDto(bookDto);
        ValidateObjectId(bookDto.Id);

        if (!await IsBookExistsAsync(bookDto.Id))
        {
            throw new NotFoundException($"Book with ID '{bookDto.Id}' not found.");
        }

        var updatedBook = await _bookRepository.UpdateBookAsync(_mapper.Map<Book>(bookDto)) ??
            throw new InvalidOperationException("Book update failed.");

        return _mapper.Map<BookDto>(updatedBook);
    }

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
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="bookDto"/> is <c>null</c> or contains no valid fields to update.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown when a book with the specified ID does not exist in the data source.
    /// </exception>
    /// <remarks>
    /// Each field is only updated if it passes the corresponding validation defined in the service.
    /// For example, string fields must not be null or whitespace, numeric fields must be positive, and collections must not be empty.
    /// </remarks>
    public async Task<BookDto> UpdateBookPartlyAsync(BookUpdateDto bookDto)
    {
        if (bookDto is null)
        {
            throw new ValidationException("Book data cannot be null.");
        }

        ValidateObjectId(bookDto.Id);

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
        updates.AddIfNotNull(bookDto.IsAvailable, b => b.IsAvailable, v => v is not null);

        if (updates.Count == 0)
        {
            throw new ValidationException("No valid fields provided for update.");
        }

        var updatedBook = await _bookRepository.UpdateBookPartlyAsync(updates, bookDto.Id);

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
    /// Validates the provided <see cref="BookDto"/> instance.
    /// </summary>
    /// <param name="bookDto">
    /// The <see cref="BookDto"/> object containing book data to validate.
    /// </param>
    /// <exception cref="ValidationException">
    /// Thrown when the <paramref name="bookDto"/> is <c>null</c> or contains invalid or missing fields.
    /// </exception>
    /// <remarks>
    /// This method performs structural and business-rule validation, including:
    /// <list type="bullet">
    /// <item><description>Non-empty title, publisher, language, and annotation</description></item>
    /// <item><description>At least one author and one genre</description></item>
    /// <item><description>Positive numeric values for price and pages</description></item>
    /// <item><description>A well-formed absolute URI for the book link</description></item>
    /// </list>
    /// </remarks>
    private static void ValidateBookDto(BookDto bookDto)
    {
        if (bookDto is null)
        {
            throw new ValidationException("Book data cannot be null.");
        }

        bool inValid = string.IsNullOrWhiteSpace(bookDto.Title) ||
            IsNullOrEmptyStringCollection(bookDto.Authors) ||
            bookDto.Price <= 0 || Decimal.TryParse(bookDto.Price.ToString(), out _) == false ||
            bookDto.Pages <= 0 || Int32.TryParse(bookDto.Pages.ToString(), out _) == false ||
            string.IsNullOrWhiteSpace(bookDto.Publisher) ||
            string.IsNullOrWhiteSpace(bookDto.Language) ||
            IsNullOrEmptyStringCollection(bookDto.Genres) ||
            string.IsNullOrWhiteSpace(bookDto.Annotation) ||
            bookDto.Link is null || !Uri.IsWellFormedUriString(bookDto.Link.ToString(), UriKind.Absolute) || string.IsNullOrWhiteSpace(bookDto.Link.ToString());

        if (inValid)
        {
            throw new ValidationException("Book fields cannot be empty.");
        }
    }

    /// <summary>
    /// Checks if a collection of strings is null, empty, or contains only whitespace in its first element.
    /// </summary>
    /// <param name="collection">
    /// IReadOnlyCollection of strings to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if the collection is null, empty, or its first element is null, empty, or whitespace; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsNullOrEmptyStringCollection(IReadOnlyCollection<string>? collection)
    {
        return collection is null || collection.Count == 0 || string.IsNullOrWhiteSpace(collection.First());
    }

    #endregion Helpers
}
