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
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        return Ok( await _service.GetBookByIdAsync(id));
    }
}
