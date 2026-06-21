using BookShop.API.Repositories;
using BookShop.API.DTOs.Order;
using BookShop.API.Exceptions;
using BookShop.API.Models.Order;
using AutoMapper;
using BookShop.API.Models.Catalog;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

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
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="userId"/> is less than or equal to 0.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown when the user's cart does not exist or is empty.
    /// </exception> 
    public async Task<OrderDto> CreateOrderAsync(int userId)
    {
        if(userId <= 0)
        {
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));
        }

        var cart = await _cartRepository.GetByUserIdAsync(userId.ToString()) 
            ?? throw new NotFoundException("Cart for required user was not found.");

        if(cart.Items.Count == 0)
        {
            throw new NotFoundException("Cart is empty.");
        }

        var result = await _orderRepository.CreateOrderAsync(CreateOrderFromCart(cart));
        return _mapper.Map<OrderDto>(result);
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
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="orderId"/> is  less than or equal to 0.
    /// </exception>
    public async Task<OrderDto?> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        if(orderId <= 0)
        {
            throw new ArgumentException("Order ID must be greater than 0.", nameof(orderId));
        }

        var result = await _orderRepository.UpdateStatusAsync(orderId, status);

        return result is null ? null : _mapper.Map<OrderDto>(result);
    }

    /// <summary>
    /// Builds an <see cref="Order"/> entity from the provided shopping cart, capturing a snapshot of each item's details at the time
    /// of order creation. 
    /// </summary>
    /// <param name="cart">
    /// The shopping cart to convert into an order.
    /// </param>
    /// <returns>
    /// A new <see cref="Order"/> with status <see cref="OrderStatus.Pending"/>.  
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="Cart.UserId"/> is null or whitespace.
    /// </exception>  
    private Order CreateOrderFromCart(Cart cart)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cart.UserId);

        decimal totalPrice = 0.0M;

        var dateTimeNow = DateTime.UtcNow;

        foreach(Item i in cart.Items)
        {
            totalPrice += i.Price * i.Quantity;
        }

        return new()
        {
          UserId = int.Parse(cart.UserId),
          Items = _mapper.Map<ICollection<OrderItem>>(cart.Items),
          TotalPrice = totalPrice,
          Status = OrderStatus.Pending,
          CreatedAt = dateTimeNow,
          UpdatedAt = dateTimeNow
        };
    }
}