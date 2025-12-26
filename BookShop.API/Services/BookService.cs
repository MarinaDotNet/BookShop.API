using AutoMapper;
using BookShop.API.Exceptions;
using BookShop.API.Models;
using BookShop.API.Repositories;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

    #endregion Getters

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

    #endregion Helpers
}
