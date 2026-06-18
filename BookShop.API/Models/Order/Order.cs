using BookShop.API.Models.Auth;

namespace BookShop.API.Models.Order;

/// <summary>
/// Represents a customer order stored in PostgreSQL.
/// </summary>
public class Order
{
    /// <summary>
    /// The unique identifier of the order.
    /// </summary>
    public int Id {get; set;}

    /// <summary>
    /// The identifier of the user who placed the order.
    /// </summary>
    public int UserId {get; set;}

    /// <summary>
    /// The user who placed the order.
    /// </summary>
    public User User {get; set;} = null!;

    /// <summary>
    /// The list of items included in the order.
    /// </summary>
    public ICollection<OrderItem> Items {get; set;} = [];

    /// <summary>
    /// The total price of the order at the time it was placed.
    /// </summary>
    public decimal TotalPrice {get; set;}

    /// <summary>
    /// The current status of the order.
    /// </summary>
    public OrderStatus Status {get; set; }

    /// <summary>
    /// The UTC timestamp when the order was placed.
    /// </summary>
    public DateTime CreatedAt {get; set; }

    /// <summary>
    /// The UTC timestamp when the order was last modified.
    /// </summary>
    public DateTime UpdatedAt {get; set; }
}