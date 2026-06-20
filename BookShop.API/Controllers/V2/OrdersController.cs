using BookShop.API.Services;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using BookShop.API.DTOs.Order;

namespace BookShop.API.Controllers.V2;

/// <summary>
/// Manages order retrieval and status opeations for authenticated users.
/// </summary>
[ApiController]
[EnableCors("PublicPolicy")]
[Authorize(Roles = "user, admin")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController(IOrderService service) : BaseApiController
{
    private readonly IOrderService _service = service;

    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="orderId">
    /// The unique identifier of the order.
    /// </param>
    /// <returns>
    /// The order is found.
    /// </returns>
    /// <response code="200">
    /// Returns the order.
    /// </response>
    /// <response code="400">
    /// The provided order ID is invalid.
    /// </response>
    /// <response code="401">
    /// The user is not authenticated.
    /// </response>
    /// <response code="404">
    /// No order with the specified ID was found.
    /// </response>
    [HttpGet("{orderId}")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById([FromRoute]int orderId)
    {
        var result = await _service.GetByIdAsync(orderId);
        return result == null 
            ? NotFound() 
            : Ok(result);
    }

    /// <summary>
    /// Retrieves all orders placed by the currenlty authenticated user.
    /// </summary>
    /// <returns>
    /// A collection of orders belonging to the user.
    /// </returns>
    /// <response code="200">
    /// Returns the user's orders, or an empty list if none exist.
    /// </response>
    /// <response code="400">
    /// The user identifier extracted from the token is invalid.
    /// </response>
    /// <response code="401">
    /// The user is not authenticated.
    /// </response>
    [HttpGet]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByUserId()
    {
        var userId = GetCurrentUserId();
        var result = await _service.GetByUserIdAsync(userId);
        return Ok(result);
    }
}