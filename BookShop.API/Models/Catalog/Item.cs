using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookShop.API.Models.Catalog;

/// <summary>
/// Embedded document representing a single book entry within a <see cref="Cart"/>.
/// </summary>
/// <remarks>
/// Title, price, and list of authors are snapshots taken when the book was added to the cart and do not reflect subsequent changes to the book catalog.
/// </remarks>
public class Item
{
    /// <summary>
    /// The identifier of the referenced book in the books collection.
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("bookId")]
    public string? BookId {get; set;}

    /// <summary>
    /// The book title at the time the book was added to the cart.
    /// </summary>
    [BsonElement("title")]
    public string? Title {get; set;}

    /// <summary>
    /// The book price at the time the book was added to the cart.
    /// </summary>
    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price {get; set;}

    /// <summary>
    /// The number of copies requested.
    /// </summary>
    [BsonElement("quantity")]
    public int Quantity {get; set;}

    /// <summary>
    /// The list of authors at the time the book was added to the cart.
    /// </summary>
    [BsonElement("authors")]
    public List<string> Authors {get; set;} = [];

}