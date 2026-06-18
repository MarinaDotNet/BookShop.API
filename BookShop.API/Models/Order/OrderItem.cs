using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.API.Models.Order;

/// <summary>
/// Represents a single book entry within an <see cref="Order"/>. 
/// </summary>
/// <remarks>
/// Title, price, and authors are snapshots taken at the time the order was placed
/// and do not reflect subsequent changes to the book catalog.
/// </remarks>
public class OrderItem
{   
    /// <summary>
    /// The unique identifier of the order item.
    /// </summary>
    public int Id {get; set; }

    /// <summary>
    /// The identifier of the order this item belongs.
    /// </summary>
    public int OrderId {get; set; }

    /// <summary>
    /// The order this item belongs to.
    /// </summary>
    public Order Order {get; set; } = null!;

    /// <summary>
    /// The identifier of the referenced book in the books collection.
    /// </summary>
    public string BookId {get; set; } = string.Empty;

    /// <summary>
    /// The book title at the time the order was placed.
    /// </summary>
    public string Title {get; set; } = string.Empty;

    /// <summary>
    /// The list of authors at the time the order was placed.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public List<string> Authors {get; set;} = [];

    /// <summary>
    /// The book price at the time the order was placed.
    /// </summary>
    public decimal Price {get; set;}

    /// <summary>
    /// The number of copies ordered.
    /// </summary>
    public int Quantity {get; set;}
}