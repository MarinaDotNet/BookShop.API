using BookShop.API.Models;
using BookShop.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

/// <summary>
/// Represents an API controller that provides endpoints for managing and retrieving books.
/// </summary>
/// <remarks>This controller is configured with the route 'books' and exposes endpoints for accessing book data.
/// It relies on dependency injection to obtain an instance of BookService.</remarks>
/// <param name="service">The service used to perform book-related operations. Cannot be null.</param>
[ApiController]
[Route("api/[controller]")]
public class BooksController(BookService service) : ControllerBase
{
    private readonly BookService _service = service;

    /// <summary>
    /// Retrieves a collection of books with optional filtering by availability.
    /// </summary>
    /// <param name="isAvailable">
    /// Optional availability filter.
    /// If null, all books are returned.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the list of books that match the specified criteria.
    /// Returned with HTTP 200 status code.
    /// </returns>
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(bool? isAvailable)
    {
        return Ok( await _service.GetAllBooksAsync(isAvailable));
    }

    /// <summary>
    /// Retrieves a book by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to retrieve.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the book with the specified identifier.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        return Ok( await _service.GetBookByIdAsync(id));
    }

    /// <summary>
    /// Retrieves books that match the specified search criteria exactly.
    /// </summary>
    /// <param name="request">
    /// The search criteria encapsulated in a <see cref="BookSearchRequestDto"/> object.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the list of books that exactly match the search criteria.
    /// </returns>
    [HttpGet("search-exact") ]
    public async Task<IActionResult> GetByExactMatch([FromQuery] BookSearchRequestDto request)
    {
        return Ok( await _service.GetBooksByExactMatchAsync(request));
    }

    /// <summary>
    /// Retrieves books that match the specified search criteria partially.
    /// </summary>
    /// <param name="request">
    /// The search criteria encapsulated in a <see cref="BookSearchRequestDto"/> object.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a read-only collection of <see cref="BookDto"/>
    /// objects that partially match the search criteria. Returns HTTP 200 status code.
    /// </returns>
    [HttpGet("search-partial-match")]
    public async Task<IActionResult> GetByPartialMatch([FromQuery] BookSearchRequestDto request)
    {
        return Ok( await _service.GetBooksByPartialMatchAsync(request));
    }


    #region Setters

    /// <summary>
    /// Adds a new book to the collection.
    /// </summary>
    /// <param name="bookDto">
    /// A <see cref="BookDto"/> object representing the book to be added.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the created <see cref="BookDto"/> object.
    /// Returns HTTP 201 (Created) with a link to the newly created resource.
    /// </returns>
    /// <remarks>
    /// Calls <see cref="BookService.CreateBookAsync"/> to perform the creation.
    /// </remarks>
    [HttpPost("add")]
    public async Task<IActionResult> CreateBook([FromBody] BookDto bookDto)
    {
        var createdBook = await _service.CreateBookAsync(bookDto);
        return CreatedAtAction(nameof(GetById), new { id = createdBook.Id }, createdBook);
    }

    #endregion
}
