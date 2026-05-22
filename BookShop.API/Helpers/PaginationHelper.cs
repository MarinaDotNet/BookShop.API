using System.ComponentModel.DataAnnotations;
using BookShop.API.DTOs.Shared;
using AutoMapper;

namespace BookShop.API.Helpers;

/// <summary>
/// Provides helper methods for pagination calculations.
/// This static class contains methods to calculate the total number of pages based on the total count of items and the page size,
/// as well as to calculate the numbe of items to skip for a given page number and page size.
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Calculates the total number of pages based on the total count of items and the page size.
    /// </summary>
    /// <param name="totalCount">
    /// The total number of items across all pages in the paginated results.
    /// </param>
    /// <param name="pageSize">
    /// The number of items included in each page of the paginated results. Must be a positive integer.
    /// </param>
    /// <returns>
    /// The total number of pages in the paginated results. This is an integer that indicates the total number of pages available
    /// in the dataset being paginated. If the page size is zero or negative, or if the total count is zero or negative, this 
    /// method returns zero, indicating that there are no pages available for pagination.
    /// </returns>
    public static int CalculateTotalPages(long totalCount, int pageSize)
    {
        if(pageSize <= 0 || totalCount <= 0)
        {
            return 0;
        }
        return (int)Math.Ceiling((double) totalCount / pageSize);
    }

    /// <summary>
    /// Calculates the number of items to skip for a given page number and page size based on the total count of items.
    /// </summary>
    /// <param name="pageNumber">
    /// The page number for which to calculate the skip value. Must be a positive integer.
    /// </param>
    /// <param name="pageSize">
    /// The number of items included in each page of the paginated results. Must be a positive integer.
    /// </param>
    /// <returns>
    /// The number of items to skip for the specified page number and page size. This is an integer that indicates the starting position
    /// of the page in the paginated results. If the page number is less than or equal to zero, or if the page size is less than or
    /// equal to zero, this method returns zero, indicating that no items should be skipped and the pagination should start from the
    /// beginning of the dataset.
    /// </returns>
    public static int CalculateSkip(int pageNumber, int pageSize)
    {
        return pageSize <= 0 || pageNumber <= 0
            ? 0 
            : (pageNumber - 1) * pageSize;
    }

    /// <summary>
    /// Maps a paginated result object from one item type to another while preserving the pagination metadata.
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of items contained in the source paginated result.
    /// </typeparam>
    /// <typeparam name="TDestination">
    /// The type of items contained in the destination paginated result.
    /// </typeparam>
    /// <param name="source">
    /// The source pagingated result containing the original items and pagination metadata.
    /// </param>
    /// <param name="items">
    /// The mapped collection of destination items.
    /// </param>
    /// <returns>
    /// A <see cref="PageResultDto{T}"/> containing the mapped items and the original pagination metadata. 
    /// </returns>
    public static PageResultDto<TDestination> MapPageResult<TSource, TDestination>(PageResultDto<TSource> source, IReadOnlyCollection<TDestination> items)
    {
        return new PageResultDto<TDestination>(
            items,
            source.PageNumber,
            source.PageSize,
            source.TotalCount,
            source.TotalPages);
    }

    /// <summary>
    /// Maps a paginated result object from one item type to another using an AutoMapper instance, while preserving the pagination metadata.
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of items contained in the source paginated result.
    /// </typeparam>
    /// <typeparam name="TDestination">
    /// The type of items contained in the destination paginated result.
    /// </typeparam>
    /// <param name="mapper">
    /// The AutoMapper instance used to map the items from the source type to the destination type. This parameter is required and must not
    /// be null. If nul, an <see cref="ArgumentNullException"/> will be thrown. 
    /// </param>
    /// <param name="source">
    /// The source paginated result containing the original items and pagination metadata. This parameter is required and must not be null.
    /// If null, an <see cref="ArgumentNullException"/> will be thrown. 
    /// </param>
    /// <returns>
    /// A <see cref="PageResultDto{TDestination}"/> containing the mapped items and the original pagination metadata. The items in the returned
    /// paginated result are mapped from the source items to the destination type using the provided AutoMapper instance. If the source paginated
    /// result contains no items, the returned paginated result will also contain an empty collection of items, but will still preserve the pagination
    /// metadata such as page number, page size, total count, and total pages from the source paginated result. 
    /// </returns>
    public static PageResultDto<TDestination> MapPageResult<TSource, TDestination>(IMapper mapper, PageResultDto<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        var items = mapper.Map<IReadOnlyCollection<TDestination>>(source.Items);
        return MapPageResult(source, items);
    }

}