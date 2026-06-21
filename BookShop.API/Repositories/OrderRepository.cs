using BookShop.API.Infrastructure.Persistence;
using BookShop.API.Models.Order;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace BookShop.API.Repositories;

/// <summary>
/// Provides data access operations for orders using Entity Framework Core.
/// </summary>
public class OrderRepository(OrderDbContext context) : IOrderRepository
{
    private readonly OrderDbContext _context = context;

    /// <summary>
    /// Retrieves an order by its identifier.
    /// </summary>
    /// <param name="orderId">
    /// The identifier of the order to retrieve.
    /// </param>
    /// <returns>
    /// The order if found; otherwise <c>null</c>.
    /// </returns>
    public async Task<Order?> GetByIdAsync(int orderId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    /// <summary>
    /// Retrieves all orders belonging to the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose orders to retrieve.
    /// </param>
    /// <returns>
    /// A collection of orders placed by the specified user. Returns an empty collection if none exist.
    /// </returns>
    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    /// <summary>
    /// Persists a new order to the database.
    /// </summary>
    /// <param name="order">
    /// The order to create. Must not be null.
    /// </param>
    /// <returns>
    /// The created order as stored in the database.
    /// </returns>
    public async Task<Order> CreateOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// Updates the status of the specified order.
    /// </summary>
    /// <param name="orderId">
    /// The identifier of the order to update.
    /// </param>
    /// <param name="status">
    /// The new status to assign to the order.
    /// </param>
    /// <returns>
    /// The updated order if found; otherwise <c>null</c>.
    /// </returns>
    public async Task<Order?> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if(order is null)
        {
            return null;
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        return order;
    }

}