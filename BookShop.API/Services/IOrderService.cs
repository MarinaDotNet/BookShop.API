using BookShop.API.DTOs.Order;
using BookShop.API.Models.Order;
using BookShop.API.Exceptions;

namespace BookShop.API.Services;

/// <summary>
/// Defines the contract for order management operations.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="orderId">
    /// The unique identifier of the order.
    /// </param>
    /// <returns>
    /// The <see cref="OrderDto"/> if found; otherwise, <c>null</c>.  
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="orderId"/> is equals to 0 or less than 0.
    /// </exception>
    Task<OrderDto?> GetByIdAsync(int orderId);

    /// <summary>
    /// Retrieves all orders placed by a specified user.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user.
    /// </param>
    /// <returns>
    /// A collection of <see cref="OrderDto"/> representing the user's orders. 
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="userId"/> is equals to 0 or less than 0.
    /// </exception>
    Task<IEnumerable<OrderDto>> GetByUserIdAsync(int userId);
    
    /// <summary>
    /// Creates a new order from the current user's shopping cart.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user placing the order.
    /// </param>
    /// <returns>
    /// The created <see cref="OrderDto"/>. 
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="userId"/> is less than or equal to 0.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown when the user's cart does not exist or is empty.
    /// </exception> 
    Task<OrderDto> CreateOrderAsync(int userId);

    /// <summary>
    /// Updates the status of an existing order.
    /// </summary>
    /// <param name="orderId">
    /// The unique identifier of the order to update.
    /// </param>
    /// <param name="status">
    /// The new status assign to the order.
    /// </param>
    /// <returns>
    /// The updated <see cref="OrderDto"/> if found; otherwise <c>null</c>. 
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="orderId"/> is  less than or equal to 0.
    /// </exception>
    Task<OrderDto?> UpdateStatusAsync(int orderId, OrderStatus status);

}