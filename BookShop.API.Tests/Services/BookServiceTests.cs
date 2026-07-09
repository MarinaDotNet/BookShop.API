namespace BookShop.API.Tests.Services;
using BookShop.API.Models.Catalog;
using BookShop.API.DTOs.Catalog;
using BookShop.API.Services;
using BookShop.API.Repositories;
using BookShop.API.Exceptions;
using AutoMapper;
using Moq;
using FluentAssertions;
using MongoDB.Bson;
using BookShop.API.DTOs.Shared;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticAssets;
using DnsClient.Protocol;
using System.Diagnostics.CodeAnalysis;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly BookService _bookService;
    private readonly CancellationToken cancellationToken = CancellationToken.None;

    public BookServiceTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _mapperMock = new Mock<IMapper>();
        _bookService = new BookService(_bookRepositoryMock.Object, _mapperMock.Object);
    }

    #region of All HttpGET Tests

    #region of GetAllBooksAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.GetAllBooksAsync(bool?, PaginationQueryDto, CancellationToken)"/> returns a paginated
    /// collection of <see cref="BookDto"/> objects when the request is valid.  
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnPagedBooks_WhenRequestIsValid()
    {
        bool isAvailable = true;

        Book book = CreateTestBook();
        PaginationQueryDto pagination = CreatePaginationQueryDto(1, 10);

        var pageResult = CreateTestPageResult(book);
        var expectedResult = CreateTestPageResult(CreateTestBookDto(book));

        _bookRepositoryMock.Setup(repo => repo
            .GetAllBooksAsync(isAvailable, pagination, cancellationToken))
            .ReturnsAsync(pageResult);

        _mapperMock.Setup(mapper => mapper
                .Map<IReadOnlyCollection<BookDto>>(It.IsAny<IReadOnlyCollection<Book>>()))
                .Returns(expectedResult.Items);

        var result = await _bookService.GetAllBooksAsync(isAvailable, pagination, cancellationToken);

        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(repo => repo.GetAllBooksAsync(isAvailable, pagination, cancellationToken), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(It.IsAny<IReadOnlyCollection<Book>>()), Times.Once);
    }
    
    /// <summary>
    /// Verifies that <see cref="BookService.GetAllBooksAsync(bool?, PaginationQueryDto, CancellationToken)"/> returns an empty 
    /// paginated result when the repository contains no books.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnEmptyPage_WhenNoBookExist()
    {
        PaginationQueryDto pagination = CreatePaginationQueryDto(1, 10);

        var pageResult = new PageResultDto<Book>([], 1, 10, 0, 0);
        var expectedResult = new PageResultDto<BookDto>([], 1, 10, 0, 0);

        _bookRepositoryMock.Setup(repo => repo
            .GetAllBooksAsync(true, pagination, cancellationToken))
            .ReturnsAsync(pageResult);

        _bookRepositoryMock.Verify(repo => 
            repo.GetAllBooksAsync(true, pagination, It.IsAny<CancellationToken>()), Times.Never);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(It.IsAny<IReadOnlyCollection<Book>>()), Times.Never);
    }
    
    #endregion of GetAllBooksAsync Tests

    #region of GetBookByIdAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.GetBookByIdAsync(string, CancellationToken)"/> returns the expected <see cref="BookDto"/>
    /// when the provided book ID is valid and the book exists. 
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation of the test operation.
    /// </returns>
    [Fact]
    public async Task GetBookByIdAsync_ShouldReturnBook_WhenBookExists()
    {
        var book = CreateTestBook();
        var expectedBookDto = CreateTestBookDto(book);
        var bookId = book.Id!;

        _bookRepositoryMock.Setup(repo => repo.GetBookByIdAsync(bookId, cancellationToken)).ReturnsAsync(book);
        _mapperMock.Setup(mapper => mapper.Map<BookDto>(book)).Returns(expectedBookDto);

        var result = await _bookService.GetBookByIdAsync(bookId, cancellationToken);

        result.Should().BeEquivalentTo(expectedBookDto);

        _bookRepositoryMock.Verify(repo => repo.GetBookByIdAsync(bookId, cancellationToken), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<BookDto>(book), Times.Once);
    }

    /// <summary>
    /// Verfifies that <see cref="BookService.GetBookByIdAsync(string, CancellationToken)"/> throws a <see cref="ValidationException"/>
    /// when the  provided book ID is null, empty or consists only of white-space characters.  
    /// </summary>
    /// <param name="bookId">
    /// A null, empty, or white-space book ID.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation of the test operation.
    /// </returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task GetBookByIdAsync_ShouldThrowValidationException_WhenIdIsNullOrWhitespace(string? bookId)
    {
        Func<Task> act = async () => await _bookService.GetBookByIdAsync(bookId!, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Book ID cannot be empty or null.");

        _bookRepositoryMock.Verify(repo => 
            repo.GetBookByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        _mapperMock.Verify(mapper => 
            mapper.Map<BookDto>(It.IsAny<Book>()), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetBookByIdAsync(string, CancellationToken)"/> throws a <see cref="ValidationException"/>
    /// when the provided book ID is not a valid MongoDB ObjectId.  
    /// </summary>
    /// <param name="bookId">
    /// An invalid book ID supplied through <see cref="TheoryAttribute"/> and <see cref="InlineDataAttribute"/>.  
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation of the test operation.
    /// </returns>
    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("507f1f77bcf86cd7994390")]      //22 simboles
    [InlineData("not-an-object-id")]
    public async Task GetBookByIdAsync_ShouldThrowValidationException_WhenIdHasInvalidFormat(string bookId)
    {
        Func<Task> act = async () => await _bookService.GetBookByIdAsync(bookId, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Invalid Book ID format.");

        _bookRepositoryMock.Verify(repo => 
            repo.GetBookByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        _mapperMock.Verify(mapper => 
            mapper.Map<BookDto>(It.IsAny<Book>()), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetBookByIdAsync(string, CancellationToken)"/> throws <see cref="NotFoundException"/>
    /// when the specified book ID is valid but no matching book exists in the repository.  
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation of the test.
    /// </returns>
    [Fact]
    public async Task GetBookByIdAsync_ShouldThrowNotFounException_WhenBookIdDoesNotExist()
    {
        var bookId = ObjectId.GenerateNewId().ToString();

        _bookRepositoryMock.Setup(repo => repo.GetBookByIdAsync(bookId, cancellationToken)).ReturnsAsync((Book?)null);

        Func<Task> act = async () => await _bookService.GetBookByIdAsync(bookId, cancellationToken);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Book with the provided ID '{bookId}' was not found.");

        _bookRepositoryMock.Verify(repo => repo.GetBookByIdAsync(bookId, cancellationToken), Times.Once);

        _mapperMock.Verify(mapper => mapper.Map<BookDto>(It.IsAny<Book>()), Times.Never);
    }

    #endregion of GetBookByIdAsync Tests

    #region of GetBooksByExactMatchAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.GetBooksByExactMatchAsync(BookSearchRequestDto, PaginationQueryDto, CancellationToken)"/>
    /// returns the expected pagianted collection of <see cref="BookDto"/> objects when the search request and pagination parameters are valid. 
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetBooksByExactMatchAsync_ShouldReturnPagedBooks_WhenRequestIsValid()
    {
        var book = CreateTestBook();
        var pagination = CreatePaginationQueryDto(1, 10);
        var pageResult = CreateTestPageResult(book);
        var expectedResult = CreateTestPageResult(CreateTestBookDto(book));
        var requestDto = CreateBookSearchRequestDto(book.Title!, book.IsAvailable);

        _bookRepositoryMock.Setup(repo => 
            repo.GetBooksByExactMatchAsync(
                book.Title!, 
                book.IsAvailable, 
                pagination, 
                cancellationToken))
            .ReturnsAsync(pageResult);

        _mapperMock
        .Setup(mapper => mapper
            .Map<IReadOnlyCollection<BookDto>>(It.IsAny<IReadOnlyCollection<Book>>()))
            .Returns(expectedResult.Items);

        var result = await _bookService.GetBooksByExactMatchAsync(requestDto, pagination, cancellationToken);
        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByExactMatchAsync(book.Title!, book.IsAvailable, pagination, cancellationToken), Times.Once);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(It.IsAny<IReadOnlyCollection<Book>>()), Times.Once);
    }

    //GetBooksByExactMatchAsync_ShouldReturnEmptyPage_WhenNoBooksMatch
    //GetBooksByExactMatchAsync_ShoudThrowValidationException_WhenRequestIsNull
    //GetBooksByExactMatchAsync_ShouldThrowValidationException_WhenSearchTermIsInvalid [Theory][InlineData] // null, empty, whitespace
    //GetBooksByExactMatchAsync_ShouldPassAvailabilityFilterToRepository // check that request.IsAvaialble sends to the Repository

    #endregion of GetBooksByExactMatchAsync Tests


    #endregion of All HttpGET Tests


    #region Private Helper Methods

    /// <summary>
    /// Creates a test book object for unit testing purposes.
    /// </summary>
    /// <returns>
    /// A new instance of the <see cref="Book"/> class with test data. 
    /// </returns>
    private static Book CreateTestBook()
    {
        var bookId = ObjectId.GenerateNewId().ToString();
        return new Book {
            Id = bookId, 
            Title = "Test Book", 
            Authors = [ "Author 1", "Author 2"], 
            Annotation = "Test Description",
            Price = 19.99m,
            Pages = 250,
            Publisher = "Test Publisher",
            Language = "english",
            Genres = [ "Fiction", "Adventure"],
            Link = new Uri("https://example.com/test-book"),
            IsAvailable = true };
    }

    /// <summary>
    /// Creates a test book DTO object for unit testing purposes.
    /// </summary>
    /// <param name="book">
    /// The <see cref="Book"/> object from which to create the DTO. 
    /// </param>
    /// <returns>
    /// A new instance of the <see cref="BookDto"/> record with test data derived from the provided <see cref="Book"/> object.  
    /// </returns>
    private static BookDto CreateTestBookDto(Book book)
    {
        return new BookDto(
            book.Id!,
            book.Title!,
            book.Authors,
            book.Price,
            book.Pages,
            book.Publisher!,
            book.Language!,
            book.Genres,
            book.Link,
            book.IsAvailable,
            book.Annotation!);   
    }

    /// <summary>
    /// Creates a paginated result containing a single test <see cref="Book"/>. 
    /// </summary>
    /// <param name="book">
    /// The test book to include in the paginated result.
    /// </param>
    /// <returns>
    /// A <see cref="PageResultDto{T}"/> containing the provided book and test pagination metadata. 
    /// </returns>
    private static PageResultDto<Book> CreateTestPageResult(Book book)
    {
        return new PageResultDto<Book>(
            [book],
            PageNumber: 1,
            PageSize: 10,
            TotalCount: 1,
            TotalPages: 1);
    }

    /// <summary>
    /// Creates a paginated result containing a single test <see cref="BookDto"/>. 
    /// </summary>
    /// <param name="bookDto">
    /// The test book DTO to include in the paginated result.
    /// </param>
    /// <returns>
    /// A <see cref="PageResultDto{T}"/> containing the provided book DTO and test pagination metadata. 
    /// </returns>
    private static PageResultDto<BookDto> CreateTestPageResult(BookDto bookDto)
    {
        return new PageResultDto<BookDto>(
            [bookDto],
            PageNumber: 1,
            PageSize: 10,
            TotalCount: 1,
            TotalPages: 1);
    }

    /// <summary>
    /// Creates a pagination query with the specified page number and page size.
    /// </summary>
    /// <param name="pageNumber">
    /// The page number to request.
    /// </param>
    /// <param name="pageSize">
    /// The number of items per page.
    /// </param>
    /// <returns>
    /// A new <see cref="PaginationQueryDto"/> instance. 
    /// </returns>
    private static PaginationQueryDto CreatePaginationQueryDto(int pageNumber, int pageSize) => 
        new (pageNumber, pageSize);

    /// <summary>
    /// Creates a <see cref="BookSearchRequestDto"/> instance for use in unit tests. 
    /// </summary>
    /// <param name="searchTerm">
    /// The search term used to find matching books.
    /// </param>
    /// <param name="isAvailable">
    /// An optional availability filter. If <see langword="null"/>, books are returned regardless of availability. 
    /// </param>
    /// <returns>
    /// A configured <see cref="BookSearchRequestDto"/> instance. 
    /// </returns>
    private static BookSearchRequestDto CreateBookSearchRequestDto(string searchTerm, bool? isAvailable = null) =>
        new (searchTerm, isAvailable);

    #endregion Private Helper Methods
}