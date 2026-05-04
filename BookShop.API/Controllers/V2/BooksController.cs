using Asp.Versioning;
using BookShop.API.DTOs.Catalog;
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
    /// <returns>
    /// A collection of available books. Returns HTTP 200 OK.
    /// </returns>
    /// <response code="200">
    /// A collection of available books is returned successfully.
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
    public async Task<IActionResult> GetAll()
    {
        return Ok( await _service.GetAllBooksAsync(true));
    }
}