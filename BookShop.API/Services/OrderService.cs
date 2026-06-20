using BookShop.API.Repositories;
using BookShop.API.DTOs.Order;
using BookShop.API.Exceptions;
using BookShop.API.Models.Order;
using AutoMapper;
using BookShop.API.Models.Catalog;

namespace BookShop.API.Services;

/// <summary>
/// Provides order management operations including cration from cart, retrieval, and status updates.
/// </summary>
/// <remarks>
/// Order cration reads the current user's shopping cart, captures a snapshot of each item's
/// details (title, authors, price) at the time order, then persists the order via <see cref="IOrderRepository"/>. 
/// The cart is not cleared automatically - that is left to the caller.
/// </remarks> 
public class OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, IMapper mapper) : IOrderService
{
    private IOrderRepository _orderRepository = orderRepository;
    private ICartRepository _cartRepository = cartRepository;
    private IMapper _mapper = mapper;

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
    public async Task<OrderDto?> GetByIdAsync(int orderId)
    {
        if(orderId <= 0)
        {
            throw new ArgumentException("Order ID must be greater than 0.", nameof(orderId));
        }
        var result = await _orderRepository.GetByIdAsync(orderId);
        return result is null 
            ? null 
            : _mapper.Map<OrderDto>(result);
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
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="userId"/> is equals to 0 or less than 0.
    /// </exception> 
    public async Task<IEnumerable<OrderDto>> GetByUserIdAsync(int userId)
    {
        if(userId <= 0)
        {
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));
        }
        var result = await _orderRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<OrderDto>>(result);
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