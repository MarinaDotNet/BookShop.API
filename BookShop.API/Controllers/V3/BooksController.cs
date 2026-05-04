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

    private const int MaxCount = 50;
    private const int MinCount = 1;
    private const int DefaultCount = 10;

    /// <summary>
    /// Retrieves the top N cheapest available books, where N is specified by the optional <paramref name="count"/> parameter.
    /// If the <paramref name="count"/> parameter is not provided or is not in the valid range, then the default value is used.
    /// Only available books are included in the result.
    /// </summary>
    /// <returns>
    /// A collection of the top N cheapest available books. Returns HTTP 200 OK.
    /// </returns>
    /// <param name="count">
    /// The number of cheapest books to retrieve. Must be betwee <see cref="MaxCount"/> and <see cref="MinCount"/> inclusive.
    /// If not provided, the default value of <see cref="DefaultCount"/> is used.   
    /// </param>
    /// <response code="200">
    /// A collection of the books is returned successfully. If no books match the criteria, 
    /// and empty collection is returned with HTTP 200 OK.
    /// </response>
    /// <response code="400">
    /// The request is invalid.
    /// This can occur when:
    /// - The provided <paramref name="count"/> parameter is not in the valid range.
    /// </response>
    [HttpGet("cheapest")]
    [MapToApiVersion("3.0")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopCheapestBooks([FromQuery] int count = DefaultCount)
    {
        ValidateCount(count);
        return Ok( await _service.GetTopCheapestBooksAsync(count, true));
    }

    /// <summary>
    /// Retrieves the top N expensive available books, where N is specified by the optional <paramref name="count"/> parameter.
    /// If the <paramref name="count"/> parameter is not provided or is not in the valid range, then the default value is used.
    /// Only available books are included in the result.
    /// </summary>
    /// <returns>
    /// A collection of the top N expensive available books. Returns HTTP 200 OK.
    /// </returns>
    /// <param name="count">
    /// The number of expensive books to retrieve. Must be betwee <see cref="MaxCount"/> and <see cref="MinCount"/> inclusive.
    /// If not provided, the default value of <see cref="DefaultCount"/> is used.   
    /// </param>
    /// <response code="200">
    /// A collection of the books is returned successfully. If no books match the criteria, 
    /// and empty collection is returned with HTTP 200 OK.
    /// </response>
    /// <response code="400">
    /// The request is invalid.
    /// This can occur when:
    /// - The provided <paramref name="count"/> parameter is not in the valid range.
    /// </response>
    [HttpGet("expensive")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopExpensiveBooks([FromQuery] int count = DefaultCount)
    {
        ValidateCount(count);
        return Ok(await _service.GetTopExpensiveBooksAsync(count, true));
    }

    /// <summary>
    /// Validates that the specified <paramref name="count" /> is within the allowed range.
    /// </summary>
    /// <param name="count">
    /// The number of items requested.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified <paramaref name="count" /> is less than <see cref="MinCount"/> or greater than <see cref="MaxCount"/>.  
    /// </exception>
    private static void ValidateCount(int count)
    {
        if(count < MinCount || count > MaxCount)
        {
            throw new ArgumentException($"Count must be between {MinCount} and {MaxCount}", nameof(count));
        }
    }
}