namespace BookShop.API.Models.Order;

/// <summary>
/// Represents the current status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// The order has been placed and is awaiting processing.
    /// </summary>
    Pending,

    /// <summary>
    /// The order is being prepared for shipment.
    /// </summary>
    Processing,

    /// <summary>
    /// The order has been shipped and is on its way.
    /// </summary>
    Shipped,

    /// <summary>
    /// The order has been delivered to the customer.
    /// </summary>
    Delivered,

    /// <summary>
    /// The order has been cancelled.
    /// </summary>
    Cancelled
}