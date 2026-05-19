using BookShop.API.DTOs.Shared;

namespace BookShop.API.DTOs.Catalog;

/// <summary>
/// Represents a data transfer object for querying books with various filtering and sorting criteria.
/// This DTO is used to encapsulate the parameters for filtering and sorting books in the catalog,
/// including the field to sort by, the sort direction, price range, and availability status. It is designed to be used in 
/// API endpoints that retrieve a list of books based on specific criteria defined by the client. The properties of this DTO are
/// optional, allowing clients to specify only the criteria they are intersted in when querying the book catalog.
/// </summary>
/// <param name="SortBy">
/// The field by which to sort the books. This is a string that indicates the property of the book entity to sort by, such as "Title",
/// "Publisher", "Price", etc. If this parameter is null or empty, the sorting will default to a predefined field, typically "Price".
/// </param>
/// <param name="Descending">
/// A boolean value indicating the sort direction. If true, the books will be sorted in descending order based on the specified SortBy field;
/// if false, the books will be sorted in ascending order. The default value is false, meaning that if this parameter is not specified,
/// the sorting will be in ascending order.
/// </param>
/// <param name="MinPrice">
/// The minimum price filter for the books. This is a nullable decimal value that specifies the lower bound of the price range for filtering
/// books. If this parameter is null, there will be no minimum price filter applied, and books of all prices will be included in the results.
/// </param>
/// <param name="MaxPrice">
/// The maximum price filter for the books. This is a nullable decimal value that specifies the upper bound of the price range for filtering
/// books. If this parameter is null, there will be no maximum price filter applied, and books of all prices will be included in the results.
/// </param>
/// <param name="IsAvailable">
/// A boolean value indicating the availability filter for the books. This is a nullable boolean that specifies whether to filter books based
/// on their availability status. If this parameter is true, only books that are currently available will be included in the results; if false,
/// only books that are currently unavailable will be included. If this parameter is null, there will be no availability filter applied, and both
/// available and unavailable books will be included in the results.
/// </param>
public sealed record BookQueryDto(
    string? SortBy = null, 
    bool Descending = false, 
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? IsAvailable = null);