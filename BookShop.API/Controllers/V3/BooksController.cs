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
public class BooksController(BookService service) : ControllerBase
{
    private readonly BookService _service = service;

    /// <summary>
    /// Retrieves a collection of the top 10 cheapest available books. This endpoint is mapped to API version 3.0 and returns 
    /// only books that are currently available. The books are sorted by price in ascending order, and only the 10 cheapest books 
    /// are returned.
    /// are currently available.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the list of the top 10 cheapest available books.
    /// Returned with HTTP 200 status code.
    /// </returns>
    [HttpGet("cheapest")]
    [MapToApiVersion("3.0")]
    public async Task<IActionResult> GetTopCheapestBooks()
    {
        return Ok( await _service.GetTopCheapestBooksAsync(10, true));
    }
}