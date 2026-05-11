namespace BookShop.API.DTOs.Shared;

/// <summary>
/// Represents a data transfer object for paginated results.
/// This DTO encapsulates the items for the current page, pagination metadata such as the current page number, page size, 
/// total number of items across all pages, and the total number of pages. It is designed to provide a standardized structure 
/// for API responses that return paginated data, allowing clients to easily navigate through large datasets by providing necessary
/// information for pagination.
/// </summary>
/// <typeparam name="T">
/// The type of items contained in the paginated result. This allows the DTO to be flexible and reusable for different types of data
/// accorss the API, such as books, authors, or any other entities that may require pagination in the response.
/// </typeparam>
/// <param name="Items">
/// The collection of items for the current page. This is a read-only collection that contains the actual data being returned in the 
/// response. The items in this collection are of the generic type T, allowing for flexibility in the types of data that can be paginated
/// and returned by the API.
/// </param>
/// <param name="PageNumber">
/// The current page number being returned in the response. This is a positive integer that indicates which page of the paginated results
/// is being provided.
/// </param>
/// <param name="PageSize">
/// The number of items included in each page of the paginated results. 
/// </param>
/// <param name="TotalCount">
/// The total number of items across all pages in the paginated results. This is a long integer that represents the total count of items
/// available in the dataset being paginated, allowing clients to understand the overall size of the dataset.
/// </param>
/// <param name="TotalPages">
/// The total number of pages in the paginated results. This is an integer that indicates the total number of pages available in the
/// dataset being paginated.
/// </param>
public sealed record PageResultDto<T>(IReadOnlyCollection<T> Items, int PageNumber, int PageSize, long TotalCount, int TotalPages);