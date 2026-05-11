using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using BookShop.API.DTOs.Catalog;
using BookShop.API.DTOs.Shared;
using BookShop.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers.V2;

/// <summary>
/// Represents an API controller that provides endpoints for managing and retrieving books.
/// This controller is part of version 2.0 of the API and is secured with role-based authorization, allowing access to users with 
/// "user" and "admin" roles. It also enables CORS for the "PublicPolicy" to allow cross-origin requests from authorized clients.
/// </summary>
/// <param name="service">
/// The service used to perform book-related operations. Cannot be null.
/// </param>
[ApiController]
[EnableCors("PublicPolicy")]
[Authorize(Roles = "user, admin")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BooksController(IBookService service) : ControllerBase
{
    private readonly IBookService _service = service;

    /// <summary>
    /// Retrieves all available books. This endpoint is mapped to API version 2.0 and returns only books that are currently available. 
    /// </summary>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <returns>
    /// A paginated collection of books that match the specified criteria. Returns HTTP 200 OK.
    /// </returns>
    /// <response code="200">
    /// A paginated collection of available books is returned successfully.
    /// If no books match the filter, an empty collection is returned with HTTP 200 status code.
    /// </response>
    /// <response code="401">
    /// The request is unauthorized. 
    /// This can occur when:
    /// - The user is not authenticated (no valid token provided),
    /// - The token is missing,
    /// - The token has expired,
    /// - The token is invalid or malformed.
    /// </response>
    /// <response code="403">
    /// The request is forbidden.
    /// This can occur when:
    /// - The user is authenticated but does not have the required role to access this endpoint.
    /// </response>
    [HttpGet("all")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]    
    public async Task<IActionResult> GetAll([FromQuery] PaginationQueryDto pagination)
    {
        return Ok( await _service.GetAllBooksAsync(true, pagination));
    }

    /// <summary>
    /// Retrieves a book by its unique identifier. This endpoint is mapped to API version 2.0 and returns the book if it is found
    /// and currently available. If the book is not found or is not available, a 404 Not Found response is returned.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to retrieve.
    /// </param>
    /// <returns>
    /// The requested book if found and available. Returns HTTP 200 OK.
    /// If the book is not found or is not available, a 404 Not Found response is returned.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when a book with the specified <paramref name="id" /> is not found or is not currently available.
    /// </exception>
    /// <response code="200">
    /// The requested book is returned successfully.
    /// </response>
    /// <response code="400">
    /// The provided identifier is invalid (e.g., null, empty, or not a valid ObjectId).
    /// </response>
    ///  <response code="401">
    /// The request is unauthorized.
    /// This can occur when:
    /// - The user is not authenticated (no valid token provided)
    /// - The token has expired
    /// - The token is invalid or malformed
    /// </response>
    /// <response code="403">
    /// The request is forbidden.
    /// This can occur when:
    /// - The user is authenticated but does not have the required role to access this endpoint.
    /// </response>
    /// <response code="404">
    /// A book with the specified identifier is not found or is not currently available.
    /// </response>
    [HttpGet("{id}")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var book = await _service.GetBookByIdAsync(id);

        if (book is null || !book.IsAvailable)
        {
            throw new KeyNotFoundException("Book not found.");
        }
        return Ok(book);
    }
    
    /// <summary>
    /// Retrieves books that exactly match the specified search criteria. This endpoint is mapped to API version 2.0 
    /// and retuns only books that are currently available. If no books match the search criteria, an empty collection is returned
    /// with HTTP 200 OK.
    /// </summary>
    /// <param name="request">
    /// The search criteria encapsulated in a <see cref="BookSearchRequestDto"/> object.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the list of books that exactly match the search criteria.
    /// </returns>
    /// <response code="200">
    /// The search was successful, and a collection of books that exactly match the specified criteria is returned.
    /// If no books match the criteria, an empty collection is returned with HTTP 200 status code.
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
    /// - The user is authenticated but does not have the required role to access this endpoint.
    /// </response>
    [HttpGet("search-exact")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByExactMatch([FromQuery] BookSearchRequestDto request)
    {
        return Ok(await _service.GetAvailableBooksByExactMatchAsync(request));
        
    }

    /// <summary>
    /// Retrieves books that partially match the specified search criteria. This endpoint is mapped to API version 2.0 and returns
    /// only books that are currently available. If no books match the search criteria, an empty collection is returned with HTTP 200 OK.
    /// </summary>
    /// <param name="request">
    /// The search criteria encapsulated in a <see cref="BookSearchRequestDto"/> object.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the list of books that partly match the search criteria.
    /// </returns>
    /// <response code="200">
    /// The search was successful, and a collection of books that partly match the specified criteria is returned.
    /// If no books match the criteria, an empty collection is returned with HTTP 200 status code.
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
    /// - The user is authenticated but does not have the required role to access this endpoint.
    /// </response>
    [HttpGet("search-partial-match")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByPartialMatch([FromQuery] BookSearchRequestDto request)
    {
        return Ok(await _service.GetAvailableBooksByPartialMatchAsync(request));
    }

}