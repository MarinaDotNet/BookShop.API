using BookShop.API.Services;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using BookShop.API.DTOs.Order;
using BookShop.API.Models.Order;

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

    /// <summary>
    /// Creates a new order from the currenlty authenticated user's shopping cart.
    /// </summary>
    /// <returns>
    /// The created order.
    /// </returns>
    /// <response code="201">
    /// Returns the newly created order.
    /// </response>
    /// <response code="400">
    /// The user identifier extracted from the token is invalid.
    /// </response>
    /// <response code="401">
    /// The user is not authenticated.
    /// </response>
    /// <response code="404">
    /// The user's cart does not exist or is empty.
    /// </response>
    [HttpPost]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateOrder()
    {
        int userId = GetCurrentUserId();
        var result = await _service.CreateOrderAsync(userId);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Updates the status of an existing order.
    /// </summary>
    /// <param name="orderId">
    /// The unique identifier of the order to update.
    /// </param>
    /// <param name="status">
    /// The new status to assign to the order.
    /// </param>
    /// <returns>
    /// The updated order.
    /// </returns>
    /// <response code="200">
    /// Returns the updated order.
    /// </response>
    /// <response code="400">
    /// The provided ID is invalid, or the provided <see cref="OrderStatus"/> value is invalid.
    /// </response>
    /// <response code="401">
    /// The user is not authenticated.
    /// </response>
    /// <response code="404">
    /// No order with the specified ID was found.
    /// </response>
    /// <response code="403">
    /// The user does not have permission to perform this action.
    /// </response>
    [HttpPut("{orderId}")]
    [Authorize(Roles = "admin")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus([FromRoute] int orderId, [FromQuery] OrderStatus status)
    {
        var result = await _service.UpdateStatusAsync(orderId, status);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    /// <summary>
    /// Cancels the specified order on behalf of the currently authenticated user.
    /// </summary>
    /// <param name="orderId">
    /// The unique identifier of the order to cancel.
    /// </param>
    /// <returns>
    /// The cancelled order.
    /// </returns>
    /// <response code="200">
    /// Returns the cancelled order.
    /// </response>
    /// <response code="400">
    /// The provided order ID is invalid.
    /// </response>
    /// <response code="401">
    /// The user is not authenticated.
    /// </response>
    /// <response code="404">
    /// No order with the specified ID was found, or the order does not belong to the current user.
    /// </response>
    [HttpPut("{orderId}/cancel")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder([FromRoute]int orderId)
    {
        var userId = GetCurrentUserId();
        var result = await _service.CancellOrderAsync(orderId, userId);
        return result is null
            ? NotFound()
            : Ok(result);
    }
}