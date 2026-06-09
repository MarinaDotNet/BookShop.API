using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookShop.API.Models.Catalog;

/// <summary>
/// Represents a shopping cart document stored in MongoDB.
/// </summary>
/// <remarks>
/// Each cart belongs to exactly one user and holds a snapshot of selected books.
/// One cart per user is enforced via a unique index on <see cref="UserId"/>.
/// </remarks>
public class Cart
{
    /// <summary>
    /// The unique identifier of the cart document.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    public string? Id {get; set;}

    /// <summary>
    /// The identifier of the user who owns this cart.
    /// References a <see cref="BookShop.API.Models.Auth.User"/> entity stored in PostgreSQL.
    /// </summary>
    [BsonElement("userId")]
    public string? UserId {get; set;}

    /// <summary>
    /// The list of books currently in the cart.
    /// </summary>
    [BsonElement("items")]
    public List<Item> Items {get; set;} = [];

    /// <summary>
    /// The UTC timestamp when the cart was created.
    /// </summary>
    [BsonElement("createdAt")]
    public DateTime CreatedAt {get; set;}

    /// <summary>
    /// The UTC timestamp when the cart was last modified.
    /// </summary>
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt {get; set;}
}