using BookShop.API.Infrastructure.Persistence;
using BookShop.API.Models.Catalog;
using MongoDB.Bson;
using MongoDB.Driver;
using BookShop.API.DTOs.Shared;
using BookShop.API.Helpers;
using BookShop.API.DTOs.Catalog;
using System.Linq.Expressions;

namespace BookShop.API.Repositories;

/// <summary>
/// Provides access to book data stored in a MongoDB database.
/// </summary>
/// <remarks>This repository implements data access operations for books using MongoDB. All methods are
/// asynchronous and thread-safe. The repository should be disposed of when no longer needed to release database
/// resources.</remarks>
/// <param name="context">The MongoDB context used to access the book collection. Cannot be null.</param>
public class BookRepository(MongoDbContext context) : IBookRepository
{
    /// <summary>
    /// Represents the MongoDB collection used to store and retrieve <see cref="Book"/> entities.
    /// </summary>
    private readonly IMongoCollection<Book> _booksCollection = context.GetCollection();

    /// <summary>
    /// Asynchronously retrieves all books from the data source, with optional filter for availability.
    /// </summary>
    /// <remarks>
    /// This method performs a read operation that returns all available books, with optional 
    /// filter for availability. 
    /// The returned collection reflects the current state of the data store at the time of the query. 
    /// The order of books in the collection is not guaranteed.
    /// </remarks>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status. 
    /// If provided, only books match the specified availability status will be returned. 
    /// If null, no avialability filter is applied.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a paginated collection 
    /// of books that match the availability filter, or all books if filter not applied. 
    /// The collection is empty if no books are found.
    /// </returns>
    public async Task<PageResultDto<Book>> GetAllBooksAsync(bool? isAvailable, PaginationQueryDto pagination)
    {
        var filter = isAvailable.HasValue
            ? Builders<Book>.Filter.Eq(b => b.IsAvailable, isAvailable.Value)
            : Builders<Book>.Filter.Empty;

        long totalCount = await _booksCollection.CountDocumentsAsync(filter);

        if(totalCount == 0)
        {
            return CreateBookPageResult([], pagination, totalCount);
        }
        
        var books = await _booksCollection
            .Find(filter)
            .Skip(PaginationHelper.CalculateSkip(pagination.PageNumber, pagination.PageSize))
            .Limit(pagination.PageSize)
            .ToListAsync();

        return CreateBookPageResult(books, pagination, totalCount);
    }

    /// <summary>
    /// Asynchronously retrieves a book with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the book to retrieve. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Book"/> with the
    /// specified identifier, or <see langword="null"/> if no matching book is found.</returns>
    public async Task<Book> GetBookByIdAsync(string id)
    {
        var book = await _booksCollection.Find(b => b.Id!.Equals(id)).FirstOrDefaultAsync();
        return book;
    }

    /// <summary>
    /// Asynchronously retrieves <see cref="Book"/> documents from MongoDB that partially match the specified
    /// <paramref name="searchTerm"/> in one of the searchable fields 
    /// (Title, Authors, Publisher, Genres, Annotation) with optional filter for availability.
    /// </summary>
    /// <param name="searchTerm">
    /// The value to search for in the searchable fields.
    /// </param>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status.
    /// If null then no availability filter is applied.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param> 
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a paginated collection of books.
    /// </returns>
    public async Task<PageResultDto<Book>> GetBooksByExactMatchAsync(string searchTerm, bool? isAvailable, PaginationQueryDto pagination)
    {
        var regex = BuildCaseInsensitiveRegex(searchTerm);

        var filters = Builders<Book>.Filter.Or(
            Builders<Book>.Filter.Regex(b => b.Title, regex),
            Builders<Book>.Filter.Regex(b => b.Language, regex),
            Builders<Book>.Filter.Regex(b => b.Publisher, regex),

            Builders<Book>.Filter.Regex("Authors", regex),
            Builders<Book>.Filter.Regex("Genres", regex)
            );

        if (isAvailable.HasValue)
        {
            var availabilityFilter =
            Builders<Book>.Filter.Eq(b => b.IsAvailable, isAvailable.Value);

            filters = Builders<Book>.Filter.And(filters, availabilityFilter);
        }

        long totalCount = await _booksCollection.CountDocumentsAsync(filters);

        if(totalCount == 0)
        {
            return CreateBookPageResult([], pagination, totalCount);
        }

        var books = await _booksCollection
            .Find(filters)
            .Skip(PaginationHelper.CalculateSkip(pagination.PageNumber, pagination.PageSize))
            .Limit(pagination.PageSize)
            .ToListAsync();

        return CreateBookPageResult(books, pagination, totalCount);
    }

    /// <summary>
    /// Asynchronously retrieves <see cref="Book"/> documents from MongoDB that contain the specified
    /// <paramref name="searchTerm"/> as a partial, case-insensitive match in one of the searchable fields
    /// (Title, Authors, Publisher, Genres, Annotation), with an optional availability filter.
    /// </summary>
    /// <param name="searchTerm">
    /// The value to search for in the searchable fields.
    /// </param>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status.
    /// If <c>null</c>, no availability filter is applied.
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param> 
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a paginated read-only collection of matching books.
    /// </returns>
    public async Task<PageResultDto<Book>> GetBooksByPartialMatchAsync(string searchTerm, bool? isAvailable, PaginationQueryDto pagination)
    {
        var regex = BuildCaseInsensitiveRegex(searchTerm);

        var filter = Builders<Book>.Filter.Or(
            Builders<Book>.Filter.Regex(b => b.Title, regex),
            Builders<Book>.Filter.Regex(b => b.Language, regex),
            Builders<Book>.Filter.Regex(b => b.Annotation, regex),
            Builders<Book>.Filter.Regex(b => b.Publisher, regex),

            Builders<Book>.Filter.Regex("Authors", regex),
            Builders<Book>.Filter.Regex("Genres", regex)
            );

        if(isAvailable.HasValue)
        {
            var availabilityFilter =
            Builders<Book>.Filter.Eq(b => b.IsAvailable, isAvailable.Value);
            filter = Builders<Book>.Filter.And(filter, availabilityFilter);
        }

        long totalCount = await _booksCollection.CountDocumentsAsync(filter);

        if(totalCount == 0)
        {
            return CreateBookPageResult([], pagination, totalCount);
        }

        var books = await _booksCollection
            .Find(filter)
            .Skip(PaginationHelper.CalculateSkip(pagination.PageNumber, pagination.PageSize))
            .Limit(pagination.PageSize)
            .ToListAsync();

        return CreateBookPageResult(books, pagination, totalCount);
    }

    /// <summary>
    /// Asynchronously checks if a book with the specified ID exists in the collection.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to check. Cannot be <c>null</c> or empty.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> representing the asynchronous operation.
    /// The task result is <c>true</c> if the book exists; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> GetBookByIdAnyAsync(string id)
    {
        var filter = Builders<Book>.Filter.Eq(b => b.Id, id);

        return await _booksCollection.Find(filter).AnyAsync();
    }

    /// <summary>
    /// Asynchronously retrieves the top N cheapest books from the data source, with an optional filter for availability.
    /// The books are sorted in ascending order by price, and only the specified number of books are returned.
    /// If the availability filter is provided, only books that match the specified availability status will be included in the 
    /// results.
    /// If no books match the criteria, an empty collection is returned.
    /// The method ensures efficient querying by leveraging MongoDB's sorting and limiting capabilities.
    /// </summary>
    /// <param name="count">
    /// The maximum number of cheapest books to retrieve. Must be a positive integer.
    /// </param>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status. 
    /// If provided, only books match the specified availability status will be returned. 
    /// If null, no avialability filter is applied.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of the top N cheapest books
    /// that match the specified criteria. The collection is empty if no matching books are found.
    /// </returns>
    public async Task<IReadOnlyCollection<Book>> GetTopCheapestBooksAsync(int count, bool? isAvailable)
    {
        var filter = isAvailable.HasValue
            ? Builders<Book>.Filter.Eq(b => b.IsAvailable, isAvailable.Value)
            : Builders<Book>.Filter.Empty;

        return await _booksCollection.Find(filter).SortBy(b => b.Price).Limit(count).ToListAsync();
    }

    /// <summary>
    /// Asynchronously retrieves the top N expensive books from the data source, with an optional filter for availability.
    /// The books are sorted in descending order by price, and only the specified number of books are returned.
    /// If the availability filter is provided, only books that match the specified availability status will be included in the
    /// results. If no books match the criteria, an empty collection is returned.
    /// </summary>
    /// <param name="count">
    /// The maximum number of expensive books to retrieve. Must be a positive integer.
    /// </param>
    /// <param name="isAvailable">
    /// An optional parameter to filter books by their availability status.
    /// If provided, only books match the specified availability status will be returned.
    /// If null, no availability filter is applied.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of the top N expensive books
    /// that match the specified criteria. The collection is empty if no matching booka are found.
    /// </returns>
    public async Task<IReadOnlyCollection<Book>> GetTopExpensiveBooksAsync(int count, bool? isAvailable)
    {
        var filter = isAvailable.HasValue
            ? Builders<Book>.Filter.Eq(b => b.IsAvailable, isAvailable.Value)
            : Builders<Book>.Filter.Empty;

        return await _booksCollection.Find(filter).SortByDescending(b => b.Price).Limit(count).ToListAsync();
    }

    /// <summary>
    /// Asynchronously retrieves a paginated collection of <see cref="Book"/> entities from the data source that match the specified
    /// filtering and sorting criteria defined in the <see cref="BookQueryDto"/>.   
    /// </summary>
    /// <param name="query">
    /// The <see cref="BookQueryDto"/> containing the filtering and sorting criteria, sort field,
    /// sort direction, price range, and availability status. Cannot be null. 
    /// </param>
    /// <param name="pagination">
    /// Pagination parameters used to control the page number and page size of the returned results.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a paginated collection of <see cref="Book"/> entities
    /// that match the specified criteria. The collection is empty if no matching books are found. 
    /// </returns>
    public async Task<PageResultDto<Book>> GetSortedAndFilteredBooksAsync(BookQueryDto query, PaginationQueryDto pagination)
    {
        var filterDefinition = BuildFilterDefinition(query);

        long totalCount = await _booksCollection.CountDocumentsAsync(filterDefinition);

        if(totalCount == 0)
        {
            return CreateBookPageResult([], pagination, totalCount);
        }
        var sortDefinition = BuildSortDefinition(query.SortBy, query.Descending);

        var books =  await _booksCollection
            .Find(filterDefinition)
            .Sort(sortDefinition)
            .Skip(PaginationHelper.CalculateSkip(pagination.PageNumber, pagination.PageSize))
            .Limit(pagination.PageSize)
            .ToListAsync();

        return CreateBookPageResult(books, pagination, totalCount);
    }
    #region Setters

    /// <summary>
    /// Asynchronously adds a new <see cref="Book"/> to the data source.
    /// </summary>
    /// <param name="book">
    /// The <see cref="Book"/> object containing the details of the book to be added.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Book}"/> representing the asynchronous operation.
    /// The task result contains the added <see cref="Book"/> object, including the generated Id.
    /// </returns>
    public async Task<Book> AddBookAsync(Book book)
    {
        await _booksCollection.InsertOneAsync(book);
        return book;
    }

    /// <summary>
    /// Asynchronously deletes the <see cref="Book"/> with the specified identifier from the data source.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the book to be deleted. Cannot be <c>null</c> or empty.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Book}"/> representing the asynchronous operation.
    /// The task result contains the deleted <see cref="Book"/> object.
    /// </returns>
    /// <remarks>
    /// If the book with the specified ID does not exist, a <see cref="Exceptions.NotFoundException"/>
    /// is typically thrown by the service layer.
    /// </remarks>
    public async Task<Book> DeleteBookByIdAsync(string id)
    {
        var filter = Builders<Book>.Filter.Eq(b => b.Id, id);

        var deletedBook = await _booksCollection.FindOneAndDeleteAsync(filter);

        return deletedBook;
    }

    /// <summary>
    /// Asynchronously updates a full <see cref="Book"/> entity in the data source.
    /// </summary>
    /// <param name="book">The <see cref="Book"/> object containing updated data. Must have a valid ID.</param>
    /// <returns>
    /// A <see cref="Task{Book}"/> representing the asynchronous operation.
    /// The task result contains the updated <see cref="Book"/> entity.
    /// </returns>
    public async Task<Book> UpdateBookAsync(Book book)
    {
        var filter = Builders<Book>.Filter.Eq(b => b.Id, book.Id);

        var options = new FindOneAndReplaceOptions<Book>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _booksCollection.FindOneAndReplaceAsync(filter, book, options);
    }

    /// <summary>
    /// Asynchronously applies partial updates to a <see cref="Book"/> using MongoDB <see cref="UpdateDefinition{Book}"/>.
    /// </summary>
    /// <param name="updates">A list of update definitions specifying the fields to update.</param>
    /// <param name="id">The unique identifier of the book to update.</param>
    /// <returns>
    /// A <see cref="Book"/> representing the asynchronous operation.
    /// The task result contains the updated <see cref="Book"/> if found; otherwise, <c>null</c>.
    /// </returns>
    public async Task<Book?> UpdateBookPartlyAsync(List<UpdateDefinition<Book>> updates, string id)
    {
        if(updates == null || updates.Count == 0)
        {
            return await _booksCollection.Find(b => b.Id!.Equals(id)).FirstOrDefaultAsync();
        }

        var filter = Builders<Book>.Filter.Eq(b => b.Id, id);

        var update = Builders<Book>.Update.Combine(updates);

        var options = new FindOneAndUpdateOptions<Book>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _booksCollection.FindOneAndUpdateAsync(filter, update, options);
    }
    #endregion Setters

    #region Helpers

    /// <summary>
    /// Normalizes the provided string by trimming any leading or trailing whitespace 
    /// and converting it to lowercase using the invariant culture.
    /// </summary>
    /// <param name="input">
    /// The string to be normalized. Must not be <c>null</c>.
    /// </param>
    /// <returns>
    /// A trimmed, lowercase version of the input string.
    /// </returns>
    /// <remarks>
    /// This method is intendent for internal use to ensure consistent, 
    /// case-insensitive string comparisons during search operations.
    /// </remarks>
    private static string NormalizeString(string input) => input.Trim().ToLower();

    /// <summary>
    /// Creates a case-insensitive MongoDB <see cref="BsonRegularExpression"/>
    /// </summary>
    /// <param name="input">
    /// The input string used to build the regular expression.
    /// </param>
    /// <returns>
    /// A case-insensitive <see cref="BsonRegularExpression"/> for MongoDB queries.
    /// </returns>
    private static BsonRegularExpression BuildCaseInsensitiveRegex(string input)
    {
        var normalized = NormalizeString(input);
        return new BsonRegularExpression(normalized, "i");
    }

    /// <summary>
    /// Creates a paginated result object containing the specified collection of books and pagination metadata.
    /// </summary>
    /// <param name="collection">
    /// The collection of books included in the current page.
    /// </param>
    /// <param name="pagination">
    /// The pagination parameters containing the current page number and page size.
    /// </param>
    /// <param name="totalCount">
    /// The total number of books matching the query before pagination is applied.
    /// </param>
    /// <returns>
    /// A <see cref="PageResultDto{T}"/> containing the paginated collection of books and realted pagination metadata. 
    /// </returns>
    private static PageResultDto<Book> CreateBookPageResult(IReadOnlyCollection<Book> collection, 
    PaginationQueryDto pagination, long totalCount)
    {
        return new PageResultDto<Book>(
            collection,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount,
            PaginationHelper.CalculateTotalPages(totalCount, pagination.PageSize)
        );
    }

    /// <summary>
    /// Builds a MongoDB <see cref="FilterDefinition{Book}"/> based on the specified filtering criteria in the <see cref="BookQueryDto"/>. 
    /// </summary>
    /// <param name="query">
    /// The <see cref="BookQueryDto"/> containing the filtering criteria, including price range and availability status.
    /// </param>
    /// <returns>
    /// A <see cref="FilterDefinition{Book}"/> that can be used in MongoDB queries to filter <see cref="Book"/> documents based on
    /// the specified criteria. The filter will include conditions for price range and availability if they are provided in the query;
    /// itherwise, those conditions will be omitted from the filter, resulting in no filtering on those fields.
    /// </returns>
    private static FilterDefinition<Book> BuildFilterDefinition(BookQueryDto query)
    {
        var availabilityFilter = query.IsAvailable.HasValue
            ? Builders<Book>.Filter.Eq(b => b.IsAvailable, query.IsAvailable.Value)
            : Builders<Book>.Filter.Empty;

        var maxPriceFilter = query.MaxPrice.HasValue
            ? Builders<Book>.Filter.Where(b => b.Price <= query.MaxPrice.Value)
            : Builders<Book>.Filter.Empty;

        var minPriceFilter = query.MinPrice.HasValue
            ? Builders<Book>.Filter.Where(b => b.Price >= query.MinPrice.Value)
            : Builders<Book>.Filter.Empty;

        return Builders<Book>.Filter.And(availabilityFilter, maxPriceFilter, minPriceFilter);
    }

    /// <summary>
    /// Builds a MongoDB <see cref="SortDefinition{TDocument}"/> based on the specified sort field and sort direction.   
    /// </summary>
    /// <param name="sortBy">
    /// The name of the field to sort by. Supported values are "Title", "Publisher", and "Price". 
    /// If null or invalid value is provided then sorting will be applied by default on the "Price" field.
    /// </param>
    /// <param name="descending">
    /// The sort direction. If <c>true</c>, the sorting will be in descending order; otherwise, it will be in ascending order.
    /// </param>
    /// <returns>
    /// A <see cref="SortDefinition{Book}"/> that can be used in MongoDB queries to sort <see cref="Book"/> documents based on the 
    /// specified field and direction.  
    /// </returns>
    private static SortDefinition<Book> BuildSortDefinition(string? sortBy, bool descending)
    {
        var fieldName = sortBy?.Trim();

        Expression<Func<Book, object>> sortExpression = fieldName switch
        {
            var s when string.Equals(s, nameof(Book.Title), StringComparison.OrdinalIgnoreCase) => b => b.Title!,
            var s when string.Equals(s, nameof(Book.Publisher), StringComparison.OrdinalIgnoreCase) => b => b.Publisher!,
            _ => b => b.Price
        };

        return descending 
            ? Builders<Book>.Sort.Descending(sortExpression)
            : Builders<Book>.Sort.Ascending(sortExpression);
    }
    #endregion Helpers
}
