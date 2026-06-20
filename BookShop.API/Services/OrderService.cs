using BookShop.API.Repositories;
using BookShop.API.DTOs.Order;
using BookShop.API.Exceptions;
using BookShop.API.Models.Order;

namespace BookShop.API.Services;

public class OrderService(IOrderRepository repository) : IOrderService
{
    private IOrderRepository _repository = repository;

     /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="orderId">
    /// The unique identifier of the order.
    /// </param>
    /// <returns>
    /// The <see cref="OrderDto"/> if found; otherwise, <c>null</c>.  
    /// </returns>
    public async Task<OrderDto?> GetByIdAsync(int orderId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves all orders placed by a specified user.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user.
    /// </param>
    /// <returns>
    /// A collection of <see cref="OrderDto"/> representing the user's orders. 
    /// </returns>
    public async Task<IEnumerable<OrderDto>> GitByUserIdAsync(int userId)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Creates a new order from the current user's shopping cart.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user placing the order.
    /// </param>
    /// <returns>
    /// The created <see cref="OrderDto"/>. 
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when the user's cart does not exist or is emptyl.
    /// </exception>
    public async Task<OrderDto> CreateOrderAsync(int userId)
    {
        throw new NotImplementedException();
    }

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
    public async Task<OrderDto?> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        throw new NotImplementedException();
    }
}