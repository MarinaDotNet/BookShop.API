using AutoMapper;
using BookShop.API.Exceptions;
using BookShop.API.Models;
using BookShop.API.Repositories;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MongoDB.Bson;

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

    #endregion Setters

    #region Helpers

    /// <summary>
    /// Validates that the specified object ID is not null, not empty, and is in a valid format.
    /// </summary>
    /// <param name="id">The object ID to validate. Must be a non-empty, non-whitespace string in a valid ObjectId format.</param>
    /// <exception cref="ValidationException">Thrown if the object ID is null, empty, or not in a valid ObjectId format.</exception>
    private static void ValidateObjectId(string id)
    {
        if(id is null || string.IsNullOrWhiteSpace(id) || string.IsNullOrEmpty(id))
        {
            throw new ValidationException("Book ID cannot be empty or null.");
        }

        if(!ObjectId.TryParse(id, out _))
        {
            throw new ValidationException("Invalid Book ID format.");
        }
    }

    private static void ValidateBookDto(BookDto bookDto)
    {
        if (bookDto is null)
        {
            throw new ValidationException("Book data cannot be null.");
        }

        bool inValid = string.IsNullOrWhiteSpace(bookDto.Title) ||
                       bookDto.Authors is null || bookDto.Authors.Count == 0 ||
                       bookDto.Price <= 0 || Decimal.TryParse(bookDto.Price.ToString(), out _) == false ||
                       bookDto.Pages <= 0 || Int32.TryParse(bookDto.Pages.ToString(), out _) == false ||
                       string.IsNullOrWhiteSpace(bookDto.Publisher) ||
                       string.IsNullOrWhiteSpace(bookDto.Language) ||
                       bookDto.Genres is null || bookDto.Genres.Count == 0 ||
                       string.IsNullOrWhiteSpace(bookDto.Annotation) ||
                       bookDto.Link is null || !Uri.IsWellFormedUriString(bookDto.Link.ToString(), UriKind.Absolute);

        if (inValid)
        {
            throw new ValidationException("Book fields cannot be empty.");
        }
    }

    #endregion Helpers
}
