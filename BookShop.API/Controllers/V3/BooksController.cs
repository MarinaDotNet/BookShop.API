using Asp.Versioning;
using BookShop.API.DTOs.Catalog;
using BookShop.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BookShop.API.Controllers.V3;

/// <summary>
/// Represents an API controller that provides endpoints for managing and retrieving books.
/// This controller is part of version 3.0 of the API and allows anonymous access to its endpoints. It also enables CORS for 
/// the "PublicPolicy" to allow cross-origin requests from authorized clients.
/// </summary>
/// <param name="service">
/// The service used to perform book-related operations. Cannot be null.
/// </param>
[ApiController]
[EnableCors("PublicPolicy")]
[AllowAnonymous]
[ApiVersion("3.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BooksController(IBookService service) : ControllerBase
{
    private readonly IBookService _service = service;

    /// <summary>
    /// Retrieves  the top 10 cheapest available books. This endpoint is mapped to API version 3.0.
    /// Books are sorted by price in ascending order.
    /// Only available books are included in the result.
    /// </summary>
    /// <returns>
    /// A collection of the top 10 cheapest available books. Returns HTTP 200 OK.
    /// </returns>
    /// <response code="200">
    /// A collection of the books is returned successfully. If no books match the criteria, 
    /// and empty collection is returned with HTTP 200 OK.
    /// </response>
    /// <response code="400">
    /// The request is invalid.
    /// </response>
    [HttpGet("cheapest")]
    [MapToApiVersion("3.0")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopCheapestBooks()
    {
        return Ok( await _service.GetTopCheapestBooksAsync(10, true));
    }
}