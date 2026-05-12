using Asp.Versioning;
using BookShop.API.DTOs.Catalog;
using BookShop.API.DTOs.Shared;
using BookShop.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers.V1;

/// <summary>
/// Represents an API controller that provides endpoints for managing and retrieving books.
/// This controller is part of version 1.0 of the API and is secured with role-based authorization, allowing access only to users 
/// with "admin" role. It also enables CORS for the "AdminPolicy" to allow cross-origin requests from authorized clients.
/// </summary>
/// <remarks>
/// This controller is configured with the route 'books' and exposes endpoints for accessing book data.
/// It relies on dependency injection to obtain an instance of BookService.
/// </remarks>
/// <param name="service">
/// The service used to perform book-related operations. Cannot be null.
/// </param>
[ApiController]
[EnableCors("AdminPolicy")]
[Authorize(Roles = "admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BooksController(IBookService service) : ControllerBase
{
    private readonly IBookService _service = service;

    #region Books: Read

    /// <summary>
    /// Retrieves a collection of books with optional filtering by availability.
    /// </summary>
    /// <param name="isAvailable">
    /// Optional availability filter.
    /// If null, all books are returned.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <returns>
    /// A paginated collection of books that match the specified criteria.
    /// Returned with HTTP 200 status code.
    /// </returns>
    /// <response code="200">
    /// A paginated collection of books is returned successfully.
    /// If no books match the filter, an empty collection is returned with HTTP 200 status code.
    /// </response>
    /// <response code="401">
    /// The request is unauthorized. 
    /// This can occur when:
    /// - The user is not authenticated (no valid token provided),
    /// - The token has expired,
    /// - The token is invalid or malformed.
    /// </response>
    /// <response code="403">
    /// The request is forbidden.
    /// This can occur when:
    /// - The user is authenticated but does not have the required "admin" role to access.
    /// </response>
    /// <response code="400">
    /// This can occur when:
    /// - <see cref="PaginationQueryDto"/> object is null.
    /// - <see cref="PaginationQueryDto.PageNumber"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> exceeds <see cref="PaginationQueryDto.MaxPageSize"/>.
    /// </response>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Tags("Books: Read")]
    public async Task<IActionResult> GetAll(bool? isAvailable, [FromQuery] PaginationQueryDto pagination)
    {
        return Ok( await _service.GetAllBooksAsync(isAvailable, pagination));
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
    /// <response code="200">
    /// The book with the specified identifier is found and returned successfully.
    /// </response>
    /// <response code="400">
    /// The provided identifier is invalid (e.g., null, empty, or not a valid ObjectId).
    /// </response>
    /// <response code="401">
    /// The request is unauthorized.
    /// This can occur when:
    /// - The user is not authenticated (no valid token provided)
    /// - The token has expired
    /// - The token is invalid or malformed
    /// </response>
    /// <response code="403">
    /// The request is forbidden.
    /// This can occur when:
    /// - The user is authenticated but does not have the required "admin" role to access this endpoint.
    /// </response>
    /// <response code="404">
    /// A book with the specified identifier is not found.
    /// </response>
    [HttpGet("{id}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Tags("Books: Read")]
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
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <returns>
    /// A paginated collection of books that exactly match the search criteria.
    /// </returns>
    /// <response code="200">
    /// The search was successful, and a paginated collection of books that exactly match the specified criteria is returned.
    /// If no books match the criteria, an empty collection is returned with HTTP 200 status code.
    /// </response>
    /// <response code="400">
    /// The search request is invalid. This can occur when:
    /// - The search term is null, empty, or contains only whitespace.
    /// - <see cref="PaginationQueryDto"/> object is null.
    /// - <see cref="PaginationQueryDto.PageNumber"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> is less then 1.
    /// - <see cref="PaginationQueryDto.PageSize"/> exceeds <see cref="PaginationQueryDto.MaxPageSize"/>.
    /// </response>
    /// <response code="401">
    /// The request is unauthorized. This can occur when:
    /// - The user is not authenticated (no valid token provided)
    /// - The token has expired
    /// - The token is invalid or malformed
    /// </response>
    /// <response code="403">
    /// The request is forbidden. This can occur when:
    /// - The user is authenticated but does not have the required "admin" role to access this endpoint.
    /// </response>
    [HttpGet("search-exact")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Tags("Books: Read")]
    public async Task<IActionResult> GetByExactMatch([FromQuery] BookSearchRequestDto request, PaginationQueryDto pagination)
    {
        return Ok( await _service.GetBooksByExactMatchAsync(request, pagination));
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
    /// <response code="200">
    /// The search was successful, and a collection of books that partly match the specified criteria is returned.
    /// If no books partly match the criteria, an empty collection is returned with HTTP 200 status code.
    /// </response>
    /// <response code="400">
    /// The search request is invalid. This can occur when:
    /// - The search term is null, empty, or contains only whitespace.
    /// </response>
    /// <response code="401">
    /// The request is unauthorized. This can occur when:
    /// - The user is not authenticated (no valid token provided)
    /// - The token has expired
    /// - The token is invalid or malformed
    /// </response>
    /// <response code="403">
    /// The request is forbidden. This can occur when:
    /// - The user is authenticated but does not have the required "admin" role to access this endpoint.
    /// </response>
    [HttpGet("search-partial-match")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Tags("Books: Read")]
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
    /// <response code="200">
    /// The existence check was successful. Returns HTTP 200 (OK) if the book exists; otherwise, returns HTTP 404 (Not Found).
    /// </response>
    /// <response code="400">
    /// The provided identifier is invalid (e.g., null, empty, or not a valid ObjectId).
    /// </response>
    /// <response code="401">
    /// The request is unauthorized. This can occur when:
    /// - The user is not authenticated (no valid token provided)
    /// - The token has expired
    /// - The token is invalid or malformed
    /// </response>
    /// <response code="403">
    /// The request is forbidden. This can occur when:
    /// - The user is authenticated but does not have the required "admin" role to access this endpoint.
    /// </response>
    /// <response code="404">
    /// A book with the specified identifier is not found.
    /// </response>
    [HttpGet("is-exists/{id}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Tags("Books: Read")]
    public async Task<IActionResult> Exists(string id)
    {
        var exists = await _service.IsBookExistsAsync(id);
        return exists ? Ok() : NotFound();
    }

    #endregion Books: Read

    #region Books: Write

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
    /// Calls <see cref="IBookService.CreateBookAsync"/> to perform the creation.
    /// </remarks>
    /// <response code="200">
    /// The book was successfully created. Returns the created <see cref="BookDto"/> object in the response body. 
    /// </response>
    /// <response code="400">
    /// The request is invalid. This can occur when:
    /// - The request body is null or missing,
    /// - The request body contains invalid data (e.g., missing required fields, invalid field values).
    /// </response>
    /// <response code="401">
    /// The request is unauthorized. This can occur when:
    /// - The user is not authenticated (no valid token provided)
    /// - The token has expired
    /// - The token is invalid or malformed
    /// </response>
    /// <response code="403">
    /// The request is forbidden. This can occur when:
    /// - The user is authenticated but does not have the required "admin" role to access this endpoint.
    /// </response>
    [HttpPost("add")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Tags("Books: Write")]
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
    /// <response code="204">
    /// The book with the specified identifier was successfully deleted. No content is returned in the response body.
    /// </response>
    /// <response code="400">
    /// The provided identifier is invalid (e.g., null, empty, or not a valid ObjectId).
    /// </response>
    /// <response code="401">
    /// The request is unauthorized. This can occur when:
    /// - The user is not authenticated (no valid token provided)
    /// - The token has expired
    /// - The token is invalid or malformed
    /// </response>
    /// <response code="403">
    /// The request is forbidden. This can occur when:
    /// - The user is authenticated but does not have the required "admin" role to access this endpoint.
    /// </response>
    /// <response code="404">
    /// A book with the specified identifier is not found.
    /// </response>
    [HttpDelete("{id}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Tags("Books: Write")]
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
    /// <response code="200">
    /// The book was successfully updated. Returns the updated <see cref="BookDto"/> object in the response body. 
    /// </response>
    /// <response code="400">
    /// The request is invalid. This can occur when:
    /// - The route ID does not match the body ID.
    /// - The request body is null or missing.
    /// - The request body contains invalid data (e.g., missing required fields, invalid field).
    /// </response>
    /// <response code="401">
    /// The request is unauthorized. This can occur when:
    /// - The user is not authenticated (no valid token provided)
    /// - The token has expired
    /// - The token is invalid or malformed
    /// </response>
    /// <response code="403">
    /// The request is forbidden. This can occur when:
    /// - The user is authenticated but does not have the required "admin" role to access this endpoint.
    /// </response>
    /// <response code="404">
    /// A book with the specified identifier is not found.
    /// </response>
    [HttpPut("{id}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Tags("Books: Write")]
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
    /// <response code="200">
    /// The book was successfully updated. Returns the updated <see cref="BookDto"/> object in the response body. 
    /// </response>
    /// <response code="400">
    /// The request is invalid. This can occur when:
    /// - The route ID does not match the body ID.
    /// - The request body is null or missing.
    /// - The request body contains invalid data (e.g., missing required fields, invalid field).
    /// </response>
    /// <response code="401">
    /// The request is unauthorized. This can occur when:
    /// - The user is not authenticated (no valid token provided)
    /// - The token has expired
    /// - The token is invalid or malformed
    /// </response>
    /// <response code="403">
    /// The request is forbidden. This can occur when:
    /// - The user is authenticated but does not have the required "admin" role to access this endpoint.
    /// </response>
    /// <response code="404">
    /// A book with the specified identifier is not found.
    /// </response>
    [HttpPatch("update-partly/{id}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Tags("Books: Write")]
    public async Task<IActionResult> UpdatePartlyBook(string id, [FromBody]BookUpdateDto bookDto)
    {
        if(!id.Equals(bookDto.Id, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Route ID does not match body ID.");
        }

        var updatedBook = await _service.UpdateBookPartlyAsync(bookDto);
        return Ok(updatedBook);
    }

    #endregion Books: Write
}
