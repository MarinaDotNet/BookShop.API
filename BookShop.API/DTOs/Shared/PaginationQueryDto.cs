namespace BookShop.API.DTOs.Shared;

/// <summary>
/// Represents a data transfer object for pagination query parameters.
/// This DTO is used to encapsulate the page number and page size for paginated API requests.
/// </summary>
/// <param name="PageNumber">
/// The page number to retrieve. Defaults to 1 if not specified. Must be a positive integer.
/// </param>
/// <param name="PageSize">
/// The number of items to include in each page. Defaults to 10 if not specified. Must be a positive integer,
/// typically with an upper limit to prevent excessive data retrieval in a single request.
/// </param>
public sealed record PaginationQueryDto(int PageNumber = 1, int PageSize = 10);