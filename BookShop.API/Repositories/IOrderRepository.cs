using BookShop.API.Models.Order;

namespace BookShop.API.Repositories;

/// <summary>
/// Defines data access operation for orders in PostgreSQL.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Retrieves an order by its identifier.
    /// </summary>
    /// <param name="orderId">
    /// The identifier of the order to retrieve.
    /// </param>
    /// <returns>
    /// The order if found; otherwise <c>null</c>.
    /// </returns>
    Task<Order?> GetByIdAsync(int orderId);

    /// <summary>
    /// Retrieves all orders belonging to the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose orders to retrieve.
    /// </param>
    /// <returns>
    /// A collection of orders placed by the specified user. Returns an empty collection if none exist.
    /// </returns>
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Persists a new order to the database.
    /// </summary>
    /// <param name="order">
    /// The order to create. Must not be null.
    /// </param>
    /// <returns>
    /// The created order as stored in the database.
    /// </returns>
    Task<Order> CreateOrderAsync(Order order);

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
    Task<Order?> UpdateStatusAsync(int orderId, OrderStatus status);
}