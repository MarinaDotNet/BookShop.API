using BookShop.API.DTOs.Catalog;
using BookShop.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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

    /// <summary>
    /// Asynchronously determines whether a book with the specified identifier exists.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to check.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating existence.
    /// </returns>
    /// <response code="200">A book with the specified identifier exists.</response>
    /// <response code="404">A book with the specified identifier does not exist.</response>
    /// <response code="400">The provided identifier is invalid.</response>
    [HttpGet("is-exists/{id}")]
    public async Task<IActionResult> Exists(string id)
    {
        var exists = await _service.IsBookExistsAsync(id);
        return exists ? Ok() : NotFound();
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

    /// <summary>
    /// Asynchronously deletes the book with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to delete.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> with HTTP 204 (No Content) if the deletion
    /// succeeds.
    /// </returns>
    /// <response code="204">The book was successfully deleted.</response>
    /// <response code="400">The provided identifier is invalid.</response>
    /// <response code="404">A book with the specified identifier was not found.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteById(string id)
    {
       await _service.DeleteBookByIdAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Updates an existing book completely using HTTP PUT.
    /// </summary>
    /// <param name="id">The unique identifier of the book to update. Must match <see cref="BookDto.Id"/>.</param>
    /// <param name="bookDto">The <see cref="BookDto"/> containing the updated book data.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the updated <see cref="BookDto"/> if successful,
    /// or <c>400 Bad Request</c> if the route ID does not match the body ID.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(string id, [FromBody]BookDto bookDto)
    {
        if (!id.Equals(bookDto.Id, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Route ID does not match body ID.");
        }

        var updatedBook = await _service.UpdateBookAsync(bookDto);
        return Ok(updatedBook);
    }

    /// <summary>
    /// Partially updates an existing book using HTTP PATCH semantics.
    /// Only fields provided in <see cref="BookUpdateDto"/> are updated.
    /// </summary>
    /// <param name="id">The unique identifier of the book to update. Must match <see cref="BookUpdateDto.Id"/>.</param>
    /// <param name="bookDto">The <see cref="BookUpdateDto"/> containing fields to update.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the updated <see cref="BookDto"/> if successful,
    /// or <c>400 Bad Request</c> if the route ID does not match the body ID.
    /// </returns>
    [HttpPatch("update-partly/{id}")]
    public async Task<IActionResult> UpdatePartlyBook(string id, [FromBody]BookUpdateDto bookDto)
    {
        if(!id.Equals(bookDto.Id, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Route ID does not match body ID.");
        }

        var updatedBook = await _service.UpdateBookPartlyAsync(bookDto);
        return Ok(updatedBook);
    }
    #endregion
}
