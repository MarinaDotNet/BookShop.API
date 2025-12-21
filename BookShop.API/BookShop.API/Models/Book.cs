using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.EntityFrameworkCore;

namespace BookShop.API.Models;

/// <summary>
/// Book entity stored in or from the MongoDB collection.
/// </summary>
/// <remarks>
/// Initialize the new instance of the <see cref="Book"/> with required parameters.
/// </remarks>
public class Book
{
    /// <summary>
    /// Gets or sets the unique identifier for the document in the MongoDB collection.
    /// </summary>
    /// <remarks>This property is mapped to the MongoDB '_id' field and is typically assigned by the database
    /// when a new document is inserted. The value is represented as a MongoDB ObjectId in string format.</remarks>
    [BsonId]
    [ReadOnly(true)]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the title associated with the entity.
    /// </summary>
    [BsonElement("title")]
    [BsonRepresentation(BsonType.String)]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the list of authors associated with the item.
    /// </summary>
    [BsonElement("authors")]
    public List<string> Authors { get; set; } = [];

    /// <summary>
    /// Gets or sets the price value associated with the entity.
    /// </summary>
    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    [Precision(18, 2)]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages in the book.
    /// </summary>
    [BsonElement("pages")]
    [BsonRepresentation(BsonType.Int32)]
    public int Pages { get; set; }

    /// <summary>
    /// Gets or sets the name of the publisher associated with the item.
    /// </summary>
    [BsonElement("publisher")]
    [BsonRepresentation(BsonType.String)]
    public string? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the language code associated with the entity.
    /// </summary>
    [BsonElement("language")]
    [BsonRepresentation(BsonType.String)]
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the list of genres associated with the item.
    /// </summary>
    [BsonElement("genres")]
    public List<string> Genres { get; set; } = [];

    /// <summary>
    /// Gets or sets the URI associated with this entity.
    /// </summary>
    [BsonElement("link")]
    public Uri Link { get; set; } = new Uri("about:blank");

    /// <summary>
    /// Gets or sets a value indicating whether the item is available.
    /// </summary>
    [BsonElement("isAvailable")]
    [BsonRepresentation(BsonType.Boolean)]
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Gets or sets the annotation text associated with the entity.
    /// </summary>
    [BsonElement("annotation")]
    [BsonRepresentation(BsonType.String)]
    public string? Annotation { get; set; }

}
