namespace BookShop.API.DTOs.Order;

/// <summary>
/// Represents an order returned to the client.
/// </summary>
/// <param name="Id">
/// The unique identifier of the order.
/// </param>
/// <param name="Items">
/// The list of items included in the order.
/// </param>
/// <param name="TotalPrice">
/// The total price of all items in the order.
/// </param>
/// <param name="Status">
/// The current status of the order.
/// </param>
/// <param name="CreatedAt">
/// The UTC date and time when the order was placed.
/// </param>
/// <param name="UpdatedAt">
/// The UTC date and time when the order was last updated.
/// </param>
public sealed record OrderDto(int Id, List<OrderItemDto> Items, decimal TotalPrice, string Status, DateTime CreatedAt, DateTime UpdatedAt);