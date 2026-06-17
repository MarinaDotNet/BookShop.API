using BookShop.API.Services;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using BookShop.API.DTOs.Catalog;
using BookShop.API.Models.Catalog;

namespace BookShop.API.Controllers.V2;

/// <summary>
/// Represents an API controller that provides endpoints for managing the shoping cart. This controller is part of version 2.0 of 
/// the API and is secured with role-based authorization, allowing access to users with "user" and "admin" roles.
/// </summary>
/// <param name="service">
/// The service used to perform cart-related operations. Cannot be null.
/// </param>
[ApiController]
[EnableCors("PublicPolicy")]
[Authorize(Roles = "user, admin")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CartsController(ICartService service) : BaseApiController
{
    private readonly ICartService _service = service;

    /// <summary>
    /// Retrieves the shopping cart of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// The cart belonging to the authenticated user, or HTTP 404 if no cart exists yet.
    /// </returns>
    /// <response code="200">
    /// The cart was found and returned successully.
    /// </response>
    /// <response code="404">
    /// No cart exists for the current user.
    /// </response>
    /// <response code="401">
    /// The request is not authenticated.
    /// </response>
    /// <response code="400">
    /// The user identifier claim is missing or invalid.
    /// </response>
    [HttpGet]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByUserId()
    {
        int userId = GetCurrentUserId();
        var result = await _service.GetByUserIdAsync(userId.ToString());

        return result == null 
            ? NotFound()
            : Ok(result);
    }

    /// <summary>
    /// Creates shopping cart for currently authenticated user.
    /// </summary>
    /// <returns>
    /// The created cart to the authenticated user, or HTTP 409 if the cart already exists.
    /// </returns>
    /// <response code="201">
    /// The cart was created successfully.
    /// </response>
    /// <response code="401">
    /// The request is not authenticated.
    /// </response>
    /// <response code="400">
    /// The user identifier claim is missing or invalid.
    /// </response>
    /// <response code="409">
    /// The cart already exists in database.
    /// </response>
    [HttpPost]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCart()
    {
        int userId = GetCurrentUserId();
        var result = await _service.CreateAsync(userId.ToString());

        return CreatedAtAction(nameof(GetByUserId), result);
    }

    /// <summary>
    /// Adds the specified book to the shopping cart of the currently authenticated user.
    /// Creates a new cart if one does not exist. Increments quantity if the book is already in the cart.
    /// </summary>
    /// <param name="addToCartDto">
    /// The book identifier and quantity to add. Must not be null.
    /// </param>
    /// <returns>
    /// The updated cart, or HTTP 404 if the specified book does not exist.
    /// </returns>
    /// <response code="200">
    /// The item was added and the updated cart is returned.
    /// </response>
    /// <response code="400">
    /// The request body is invalid or the book is not available.
    /// </response>
    /// <response code="401">
    /// The request is not authenticated.
    /// </response>
    /// <response code="404">
    /// The book with the specified ID was not found.
    /// </response>
    [HttpPost("items")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem([FromBody]AddToCartDto addToCartDto)
    {
        string userId = GetCurrentUserId().ToString();
        var result = await _service.AddItemAsync(userId, addToCartDto);
        return Ok(result);
    }

    /// <summary>
    /// Updates the quantity of a specific item in the shopping cart of the currently authenticated user.
    /// </summary>
    /// <param name="bookId">
    /// The identifier of the book whose quantity to update.
    /// </param>
    /// <param name="quantity">
    /// The new quantity. Must not be null and greater than zero.
    /// </param>
    /// <returns>
    /// The updated cart, or HTTP 404 if the cart or the specified item does not exist.
    /// </returns>
    /// <response code="200">
    /// The item quantity was updated and the updated cart is returned.
    /// </response>
    /// <response code="400">
    /// The request is invalid or the quantity is less than zero.
    /// </response>
    /// <response code="401">
    /// The request is not authenticated.
    /// </response>
    /// <response code="404">
    /// The cart or the specified item was not found.
    /// </response>
    [HttpPut("items/{bookId}")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItemQuantity([FromRoute]string bookId, UpdateItemQuantityDto quantity)
    {
        string userId = GetCurrentUserId().ToString();
        var result = await _service.UpdateItemQuantityAsync(userId, bookId, quantity);
        return Ok(result);
    }

    /// <summary>
    /// Removes a specific item from the shopping cart or the currently authenticated user.
    /// </summary>
    /// <param name="bookId">
    /// The identifier of the book to remove from the cart.
    /// </param>
    /// <returns>
    /// The udpated cart, or HTTP 404 if the cart was not found.
    /// </returns>
    /// <response code="200">
    /// The item was removed and the updated cart is returned.
    /// </response>
    /// <response code="400">
    /// The user identifier claim is missing or invalid.
    /// </response>
    /// <response code="401">
    /// The request is not authenticated.
    /// </response>
    /// <response code="404">
    /// The cart was not found.
    /// </response>
    [HttpDelete("items/{bookId}")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem([FromRoute]string bookId)
    {
        string userId = GetCurrentUserId().ToString();
        var result = await _service.RemoveItemAsync(userId, bookId);
        return Ok(result);
    }

    /// <summary>
    /// Deletes the shopping cart of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// HTTP 204 No Content on success, or HTTP 404 if the cart does not exist.
    /// </returns>
    /// <response code="204">
    /// The cart was successfully deleted.
    /// </response>
    /// <response code="400">
    /// The user identifier claim is missing or invalid.
    /// </response>
    /// <response code="401">
    /// The request is not authenticated.
    /// </response>
    /// <response code="404">
    /// The cart was not found.
    /// </response>
    [HttpDelete]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Clear()
    {
        string userId = GetCurrentUserId().ToString();
        await _service.ClearAsync(userId);
        return NoContent();
    }
}