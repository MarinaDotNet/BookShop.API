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
using MongoDB.Driver;

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

    #region GET Methods Tests

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

        _mapperMock.Setup(mapper =>
            mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items))
            .Returns(expectedResult.Items);

        var result = await _bookService.GetAllBooksAsync(true, pagination, cancellationToken);

        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(repo => 
            repo.GetAllBooksAsync(true, pagination, It.IsAny<CancellationToken>()), Times.Once);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items), Times.Once);
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
        Func<Task> act = () => _bookService.GetBookByIdAsync(bookId!, cancellationToken);

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
        Func<Task> act = () => _bookService.GetBookByIdAsync(bookId, cancellationToken);

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

        Func<Task> act = () => _bookService.GetBookByIdAsync(bookId, cancellationToken);

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

    /// <summary>
    /// Verifies that an empty paginated result is returned when no books match the exact search term.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetBooksByExactMatchAsync_ShouldReturnEmptyPage_WhenNoBooksMatch()
    {
        string searchTerm = "Clean Code";
        var requestDto = CreateBookSearchRequestDto(searchTerm, true);
        var pagination = CreatePaginationQueryDto(1, 10);
        
        var pageResult = new PageResultDto<Book>([], 1, 10, 0, 0);
        var expectedResult = new PageResultDto<BookDto>([], 1, 10, 0, 0);

        _bookRepositoryMock
            .Setup(repo => repo.GetBooksByExactMatchAsync(
                searchTerm, 
                true, 
                pagination, 
                cancellationToken))
            .ReturnsAsync(pageResult);

        _mapperMock
            .Setup(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items))
            .Returns([]);

        var result = await _bookService.GetBooksByExactMatchAsync(requestDto, pagination, cancellationToken);

        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(
            repo => repo.GetBooksByExactMatchAsync(
                searchTerm, 
                true, 
                pagination, 
                cancellationToken)
            ,Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items), 
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetBooksByExactMatchAsync"/> throws <see cref="ValidationException"/> 
    /// when the request is null.  
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetBooksByExactMatchAsync_ShoudThrowValidationException_WhenRequestIsNull()
    {
        var pagination = CreatePaginationQueryDto(1, 10);
        BookSearchRequestDto? request = null;

        Func<Task> act = () => _bookService.GetBooksByExactMatchAsync(request!, pagination, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Search term cannot be null.");

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByExactMatchAsync(
                It.IsAny<string>(), 
                It.IsAny<bool>(), 
                It.IsAny<PaginationQueryDto>(), 
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(
                It.IsAny<IReadOnlyCollection<BookDto>>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetBooksByExactMatchAsync(BookSearchRequestDto, PaginationQueryDto, CancellationToken)"/>
    /// throws <see cref="ValidationException"/> when the search term is null, empty, or consists only of whitespace.  
    /// </summary>
    /// <param name="searchTerm">
    /// The invalid search term supplied to the request.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task GetBooksByExactMatchAsync_ShouldThrowValidationException_WhenSearchTermIsInvalid(string? searchTerm)
    {
        var pagination = CreatePaginationQueryDto(1, 10);
        var request = CreateBookSearchRequestDto(searchTerm!, true);

        Func<Task> act = () => _bookService.GetBooksByExactMatchAsync(request, pagination, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Search term cannot be null or empty.");

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByExactMatchAsync(
                It.IsAny<string>(), 
                It.IsAny<bool>(), 
                It.IsAny<PaginationQueryDto>(), 
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(
                It.IsAny<IReadOnlyCollection<BookDto>>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetBooksByExactMatchAsync(BookSearchRequestDto, PaginationQueryDto, CancellationToken)"/>
    /// passes the availability filter from the request to the <see cref="BookRepository.GetBooksByExactMatchAsync(string, bool?, PaginationQueryDto, CancellationToken)"/>.  
    /// </summary>
    /// <param name="isAvailable">
    /// The availability filter that should be forwarded to the repository.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public async Task GetBooksByExactMatchAsync_ShouldPassAvailabilityFilterToRepository(bool? isAvailable)
    {
        var request = CreateBookSearchRequestDto("Clean Code", isAvailable);
        var pagination = CreatePaginationQueryDto(1, 10);

        var pageResult = new PageResultDto<Book>([], 1, 10, 0, 0);
        var expectedResult = new PageResultDto<BookDto>([], 1, 10, 0, 0);

        _bookRepositoryMock
            .Setup(repo => repo.GetBooksByExactMatchAsync(
                request.SearchTerm,
                isAvailable,
                pagination,
                cancellationToken))
            .ReturnsAsync(pageResult);
        
        _mapperMock
            .Setup(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items))
            .Returns(expectedResult.Items);

        var result = await _bookService.GetBooksByExactMatchAsync(request, pagination, cancellationToken);

        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByExactMatchAsync(
                request.SearchTerm,
                isAvailable,
                pagination,
                cancellationToken),
                Times.Once);

        _mapperMock.Verify(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items), Times.Once);
    }

    #endregion of GetBooksByExactMatchAsync Tests

    #region of GetAvailableBooksByExactMatchAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.GetAvailableBooksByExactMatchAsync(BookSearchRequestDto, PaginationQueryDto, CancellationToken)"/>
    /// returns the expected pagianted collection of <see cref="BookDto"/> objects when the search request and pagination parameters are valid. 
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetAvailableBooksByExactMatchAsync_ShouldReturnPagedBooks_WhenRequestIsValid()
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

        var result = await _bookService.GetAvailableBooksByExactMatchAsync(requestDto, pagination, cancellationToken);
        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByExactMatchAsync(book.Title!, book.IsAvailable, pagination, cancellationToken), Times.Once);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(It.IsAny<IReadOnlyCollection<Book>>()), Times.Once);
    }

    /// <summary>
    /// Verifies that an empty paginated result is returned when no books match the exact search term.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetAvailableBooksByExactMatchAsync_ShouldReturnEmptyPage_WhenNoBooksMatch()
    {
        string searchTerm = "Clean Code";
        var requestDto = CreateBookSearchRequestDto(searchTerm, true);
        var pagination = CreatePaginationQueryDto(1, 10);
        
        var pageResult = new PageResultDto<Book>([], 1, 10, 0, 0);
        var expectedResult = new PageResultDto<BookDto>([], 1, 10, 0, 0);

        _bookRepositoryMock
            .Setup(repo => repo.GetBooksByExactMatchAsync(
                searchTerm, 
                true, 
                pagination, 
                cancellationToken))
            .ReturnsAsync(pageResult);

        _mapperMock
            .Setup(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items))
            .Returns([]);

        var result = await _bookService.GetAvailableBooksByExactMatchAsync(requestDto, pagination, cancellationToken);

        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(
            repo => repo.GetBooksByExactMatchAsync(
                searchTerm, 
                true, 
                pagination, 
                cancellationToken)
            ,Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items), 
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetAvailableBooksByExactMatchAsync"/> throws <see cref="ValidationException"/> 
    /// when the request is null.  
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetAvailableBooksByExactMatchAsync_ShoudThrowValidationException_WhenRequestIsNull()
    {
        var pagination = CreatePaginationQueryDto(1, 10);
        BookSearchRequestDto? request = null;

        Func<Task> act = () => _bookService.GetAvailableBooksByExactMatchAsync(request!, pagination, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Search term cannot be null.");

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByExactMatchAsync(
                It.IsAny<string>(), 
                It.IsAny<bool>(), 
                It.IsAny<PaginationQueryDto>(), 
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(
                It.IsAny<IReadOnlyCollection<BookDto>>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetAvailableBooksByExactMatchAsync(BookSearchRequestDto, PaginationQueryDto, CancellationToken)"/>
    /// throws <see cref="ValidationException"/> when the search term is null, empty, or consists only of whitespace.  
    /// </summary>
    /// <param name="searchTerm">
    /// The invalid search term supplied to the request.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task GetAvailableBooksByExactMatchAsync_ShouldThrowValidationException_WhenSearchTermIsInvalid(string? searchTerm)
    {
        var pagination = CreatePaginationQueryDto(1, 10);
        var request = CreateBookSearchRequestDto(searchTerm!, true);

        Func<Task> act = () => _bookService.GetAvailableBooksByExactMatchAsync(request, pagination, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Search term cannot be null or empty.");

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByExactMatchAsync(
                It.IsAny<string>(), 
                It.IsAny<bool>(), 
                It.IsAny<PaginationQueryDto>(), 
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(
                It.IsAny<IReadOnlyCollection<BookDto>>()),
            Times.Never);
    }

    #endregion of GetAvailableBooksByExactMatchAsync Tests

    #region of GetBooksByPartialMatchAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.GetBooksByPartialMatchAsync(BookSearchRequestDto, PaginationQueryDto, CancellationToken)"/>
    /// returns the expected pagianted collection of <see cref="BookDto"/> objects when the search request and pagination parameters are valid. 
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetBooksByPartialMatchAsync_ShouldReturnPagedBooks_WhenRequestIsValid()
    {
        var book = CreateTestBook();
        var pagination = CreatePaginationQueryDto(1, 10);
        var pageResult = CreateTestPageResult(book);
        var expectedResult = CreateTestPageResult(CreateTestBookDto(book));
        string searchTerm = book.Title![2..];
        var requestDto = CreateBookSearchRequestDto(searchTerm, book.IsAvailable);

        _bookRepositoryMock.Setup(repo => 
            repo.GetBooksByPartialMatchAsync(
                searchTerm, 
                book.IsAvailable, 
                pagination, 
                cancellationToken))
            .ReturnsAsync(pageResult);

        _mapperMock
        .Setup(mapper => mapper
            .Map<IReadOnlyCollection<BookDto>>(It.IsAny<IReadOnlyCollection<Book>>()))
            .Returns(expectedResult.Items);

        var result = await _bookService.GetBooksByPartialMatchAsync(requestDto, pagination, cancellationToken);
        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByPartialMatchAsync(searchTerm, book.IsAvailable, pagination, cancellationToken), Times.Once);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(It.IsAny<IReadOnlyCollection<Book>>()), Times.Once);
    }

    /// <summary>
    /// Verifies that an empty paginated result is returned when no books match the partial search term.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetBooksByPartialMatchAsync_ShouldReturnEmptyPage_WhenNoBooksMatch()
    {
        string searchTerm = "ean Co";
        var requestDto = CreateBookSearchRequestDto(searchTerm, true);
        var pagination = CreatePaginationQueryDto(1, 10);
        
        var pageResult = new PageResultDto<Book>([], 1, 10, 0, 0);
        var expectedResult = new PageResultDto<BookDto>([], 1, 10, 0, 0);

        _bookRepositoryMock
            .Setup(repo => repo.GetBooksByPartialMatchAsync(
                searchTerm, 
                true, 
                pagination, 
                cancellationToken))
            .ReturnsAsync(pageResult);

        _mapperMock
            .Setup(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items))
            .Returns([]);

        var result = await _bookService.GetBooksByPartialMatchAsync(requestDto, pagination, cancellationToken);

        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(
            repo => repo.GetBooksByPartialMatchAsync(
                searchTerm, 
                true, 
                pagination, 
                cancellationToken)
            ,Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items), 
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetBooksByPartialMatchAsync"/> throws <see cref="ValidationException"/> 
    /// when the request is null.  
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetBooksByPartialMatchAsync_ShoudThrowValidationException_WhenRequestIsNull()
    {
        var pagination = CreatePaginationQueryDto(1, 10);
        BookSearchRequestDto? request = null;

        Func<Task> act = () => _bookService.GetBooksByPartialMatchAsync(request!, pagination, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Search term cannot be null.");

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByPartialMatchAsync(
                It.IsAny<string>(), 
                It.IsAny<bool>(), 
                It.IsAny<PaginationQueryDto>(), 
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(
                It.IsAny<IReadOnlyCollection<BookDto>>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetBooksByPartialMatchAsync(BookSearchRequestDto, PaginationQueryDto, CancellationToken)"/>
    /// throws <see cref="ValidationException"/> when the search term is null, empty, or consists only of whitespace.  
    /// </summary>
    /// <param name="searchTerm">
    /// The invalid search term supplied to the request.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task GetBooksByPartialMatchAsync_ShouldThrowValidationException_WhenSearchTermIsInvalid(string? searchTerm)
    {
        var pagination = CreatePaginationQueryDto(1, 10);
        var request = CreateBookSearchRequestDto(searchTerm!, true);

        Func<Task> act = () => _bookService.GetBooksByPartialMatchAsync(request, pagination, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Search term cannot be null or empty.");

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByPartialMatchAsync(
                It.IsAny<string>(), 
                It.IsAny<bool>(), 
                It.IsAny<PaginationQueryDto>(), 
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(
                It.IsAny<IReadOnlyCollection<BookDto>>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetBooksByPartialMatchAsync(BookSearchRequestDto, PaginationQueryDto, CancellationToken)"/>
    /// passes the availability filter from the request to the <see cref="BookRepository.GetBooksByPartialMatchAsync(string, bool?, PaginationQueryDto, CancellationToken)"/>.  
    /// </summary>
    /// <param name="isAvailable">
    /// The availability filter that should be forwarded to the repository.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public async Task GetBooksByPartialMatchAsync_ShouldPassAvailabilityFilterToRepository(bool? isAvailable)
    {
        var request = CreateBookSearchRequestDto("ean Co", isAvailable);
        var pagination = CreatePaginationQueryDto(1, 10);

        var pageResult = new PageResultDto<Book>([], 1, 10, 0, 0);
        var expectedResult = new PageResultDto<BookDto>([], 1, 10, 0, 0);

        _bookRepositoryMock
            .Setup(repo => repo.GetBooksByPartialMatchAsync(
                request.SearchTerm,
                isAvailable,
                pagination,
                cancellationToken))
            .ReturnsAsync(pageResult);
        
        _mapperMock
            .Setup(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items))
            .Returns(expectedResult.Items);

        var result = await _bookService.GetBooksByPartialMatchAsync(request, pagination, cancellationToken);

        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByPartialMatchAsync(
                request.SearchTerm,
                isAvailable,
                pagination,
                cancellationToken),
                Times.Once);

        _mapperMock.Verify(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items), Times.Once);
    }

    #endregion of GetBooksByPartialMatchAsync Tests

    #region of GetAvailableBooksByPartialMatchAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.GetAvailableBooksByPartialMatchAsync(BookSearchRequestDto, PaginationQueryDto, CancellationToken)"/>
    /// returns the expected pagianted collection of <see cref="BookDto"/> objects when the search request and pagination parameters are valid. 
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetAvailableBooksByPartialMatchAsync_ShouldReturnPagedBooks_WhenRequestIsValid()
    {
        var book = CreateTestBook();
        var pagination = CreatePaginationQueryDto(1, 10);
        var pageResult = CreateTestPageResult(book);
        var expectedResult = CreateTestPageResult(CreateTestBookDto(book));
        string searchTerm = book.Title![2..];
        var requestDto = CreateBookSearchRequestDto(searchTerm, book.IsAvailable);

        _bookRepositoryMock.Setup(repo => 
            repo.GetBooksByPartialMatchAsync(
                searchTerm, 
                book.IsAvailable, 
                pagination, 
                cancellationToken))
            .ReturnsAsync(pageResult);

        _mapperMock
        .Setup(mapper => mapper
            .Map<IReadOnlyCollection<BookDto>>(It.IsAny<IReadOnlyCollection<Book>>()))
            .Returns(expectedResult.Items);

        var result = await _bookService.GetAvailableBooksByPartialMatchAsync(requestDto, pagination, cancellationToken);
        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByPartialMatchAsync(searchTerm, book.IsAvailable, pagination, cancellationToken), Times.Once);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(It.IsAny<IReadOnlyCollection<Book>>()), Times.Once);
    }

    /// <summary>
    /// Verifies that an empty paginated result is returned when no books match the partial search term.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetAvailableBooksByPartialMatchAsync_ShouldReturnEmptyPage_WhenNoBooksMatch()
    {
        string searchTerm = "ean Co";
        var requestDto = CreateBookSearchRequestDto(searchTerm, true);
        var pagination = CreatePaginationQueryDto(1, 10);
        
        var pageResult = new PageResultDto<Book>([], 1, 10, 0, 0);
        var expectedResult = new PageResultDto<BookDto>([], 1, 10, 0, 0);

        _bookRepositoryMock
            .Setup(repo => repo.GetBooksByPartialMatchAsync(
                searchTerm, 
                true, 
                pagination, 
                cancellationToken))
            .ReturnsAsync(pageResult);

        _mapperMock
            .Setup(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items))
            .Returns([]);

        var result = await _bookService.GetAvailableBooksByPartialMatchAsync(requestDto, pagination, cancellationToken);

        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock.Verify(
            repo => repo.GetBooksByPartialMatchAsync(
                searchTerm, 
                true, 
                pagination, 
                cancellationToken)
            ,Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<IReadOnlyCollection<BookDto>>(pageResult.Items), 
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetAvailableBooksByPartialMatchAsync"/> throws <see cref="ValidationException"/> 
    /// when the request is null.  
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task GetAvailableBooksByPartialMatchAsync_ShoudThrowValidationException_WhenRequestIsNull()
    {
        var pagination = CreatePaginationQueryDto(1, 10);
        BookSearchRequestDto? request = null;

        Func<Task> act = () => _bookService.GetAvailableBooksByPartialMatchAsync(request!, pagination, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Search term cannot be null.");

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByPartialMatchAsync(
                It.IsAny<string>(), 
                It.IsAny<bool>(), 
                It.IsAny<PaginationQueryDto>(), 
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(
                It.IsAny<IReadOnlyCollection<BookDto>>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetAvailableBooksByPartialMatchAsync(BookSearchRequestDto, PaginationQueryDto, CancellationToken)"/>
    /// throws <see cref="ValidationException"/> when the search term is null, empty, or consists only of whitespace.  
    /// </summary>
    /// <param name="searchTerm">
    /// The invalid search term supplied to the request.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task GetAvailableBooksByPartialMatchAsync_ShouldThrowValidationException_WhenSearchTermIsInvalid(string? searchTerm)
    {
        var pagination = CreatePaginationQueryDto(1, 10);
        var request = CreateBookSearchRequestDto(searchTerm!, true);

        Func<Task> act = () => _bookService.GetAvailableBooksByPartialMatchAsync(request, pagination, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Search term cannot be null or empty.");

        _bookRepositoryMock.Verify(repo => 
            repo.GetBooksByPartialMatchAsync(
                It.IsAny<string>(), 
                It.IsAny<bool>(), 
                It.IsAny<PaginationQueryDto>(), 
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mapperMock.Verify(mapper => 
            mapper.Map<IReadOnlyCollection<BookDto>>(
                It.IsAny<IReadOnlyCollection<BookDto>>()),
            Times.Never);
    }

    #endregion of GetAvailableBooksByPartialMatchAsync Tests

    #region of IsBookExistsAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.IsBookExistsAsync(string, CancellationToken)"/> returns <see langword="true"/> when
    /// the specified book exists. 
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task IsBookExistsAsync_ShoulReturnTrue_WhenBookExists()
    {
        var book = CreateTestBook();
        var bookId = book.Id!;

        _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(bookId, cancellationToken))
            .ReturnsAsync(book);

        var result = await _bookService.IsBookExistsAsync(bookId, cancellationToken);

        result.Should().BeTrue();

        _bookRepositoryMock.Verify(repo => repo.GetBookByIdAsync(bookId, cancellationToken), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<BookDto>(It.IsAny<Book>()), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.IsBookExistsAsync(string, CancellationToken)"/> returns <see langword="false"/> when
    /// the specified book doesnot exists. 
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task IsBookExistsAsync_ShouldReturnFalse_WhenBookDoesNotExist()
    {
        var bookId = ObjectId.GenerateNewId().ToString();

        _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(bookId, cancellationToken))
            .ReturnsAsync((Book?)null);

        var result = await _bookService.IsBookExistsAsync(bookId, cancellationToken);

        result.Should().BeFalse();

        _bookRepositoryMock.Verify(repo => repo.GetBookByIdAsync(bookId, cancellationToken), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<BookDto>(It.IsAny<Book>()), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.IsBookExistsAsync(string, CancellationToken)"/> throws and <see cref="ValidationException"/>
    /// when the book ID is <see langword="null"/>, empty, or consists only of whitespace characters.
    /// </summary>
    /// <param name="bookId">
    /// The book ID passed to the service method.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task IsBookExistsAsync_ShouldThrowValidationException_WhenIdIsNullOrWhitespace(string? bookId)
    {
        Func<Task> act = () => _bookService.IsBookExistsAsync(bookId!, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Book ID cannot be empty or null.");

        _bookRepositoryMock.Verify(repo => repo.GetBookByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        _mapperMock.Verify(mapper => mapper.Map<BookDto>(It.IsAny<Book>()), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.IsBookExistsAsync(string, CancellationToken)"/> throws and <see cref="ValidationException"/>
    /// when the provided book ID is not a valid MonogDB ObjectId.
    /// </summary>
    /// <param name="bookId">
    /// The invalid book ID passed to the service method.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("507f1f77bcf86cd7994390")]
    [InlineData("not-an-object-id")]
    public async Task IsBookExistsAsync_ShouldThrowValidationException_WhenIdIsInvalid(string bookId)
    {
        Func<Task> act = () => _bookService.IsBookExistsAsync(bookId, cancellationToken);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Invalid Book ID format.");

        _bookRepositoryMock.Verify(repo => repo.GetBookByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        _mapperMock.Verify(mapper => mapper.Map<BookDto>(It.IsAny<Book>()), Times.Never);
    }  

    #endregion of IsBookExistsAsync Tests

    #region of GetTopCheapestBooksAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.GetTopCheapestBooksAsync(int, bool?, CancellationToken)"/> passes the availability filter
    /// to the repository and returns the mapped collection of books. 
    /// </summary>
    /// <param name="isAvailable">
    /// The availability filter passed to the service method.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public async Task GetTopCheapestBooksAsync_ShouldPassAvailabilityFilterToRepository(bool? isAvailable)
    {
        int count = 10;
        var (books, expectedResult) = CreateBooksAndDtos(count, isAvailable);

        _bookRepositoryMock.
            Setup(repo => repo.GetTopCheapestBooksAsync(count, isAvailable, cancellationToken))
            .ReturnsAsync(books);
        _mapperMock
            .Setup(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(books))
            .Returns(expectedResult);

        var result = await _bookService.GetTopCheapestBooksAsync(count, isAvailable, cancellationToken);
        result.Should().BeEquivalentTo(expectedResult);
        _bookRepositoryMock
            .Verify(repo => repo.GetTopCheapestBooksAsync(count, isAvailable, cancellationToken), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(books), Times.Once);          
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetTopCheapestBooksAsync(int, bool?, CancellationToken)"/> throws a 
    /// <see cref="ValidationExcepiton"/> when the specified count is less than or equal to zero.
    /// </summary>
    /// <param name="count">
    /// The invalid number of books requested.
    /// </param>
    /// <returns>
    /// A task representing the asynchrounous test operation.
    /// </returns>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetTopCheapestBooksAsync_ShouldThrowValidationException_WhenCountIsNotPositive(int count)
    {
        Func<Task> act = () => _bookService.GetTopCheapestBooksAsync(count, true, cancellationToken);

        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Count must be a positive integer.");
        
        _bookRepositoryMock
            .Verify(repo => repo.GetTopCheapestBooksAsync(
                It.IsAny<int>(), 
                It.IsAny<bool>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);

        _mapperMock
        .Verify(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(
            It.IsAny<Book>()), 
        Times.Never);
    }

    #endregion of GetTopCheapestBooksAsync Tests
    
    #region of GetTopExpensiveBooksAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.GetTopExpensiveBooksAsync(int, bool?, CancellationToken)"/> passes the availability filter
    /// to the repository and returns the mapped collection of books. 
    /// </summary>
    /// <param name="isAvailable">
    /// The availability filter passed to the service method.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public async Task GetTopExpensiveBooksAsync_ShouldPassAvailabilityFilterToRepository(bool? isAvailable)
    {
        int count = 10;
        var (books, expectedResult) = CreateBooksAndDtos(count, isAvailable);

        _bookRepositoryMock.
            Setup(repo => repo.GetTopExpensiveBooksAsync(count, isAvailable, cancellationToken))
            .ReturnsAsync(books);
        _mapperMock
            .Setup(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(books))
            .Returns(expectedResult);

        var result = await _bookService.GetTopExpensiveBooksAsync(count, isAvailable, cancellationToken);
        result.Should().BeEquivalentTo(expectedResult);
        _bookRepositoryMock
            .Verify(repo => repo.GetTopExpensiveBooksAsync(count, isAvailable, cancellationToken), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(books), Times.Once);          
    }

    /// <summary>
    /// Verifies that <see cref="BookService.GetTopExpensiveBooksAsync(int, bool?, CancellationToken)"/> throws a 
    /// <see cref="ValidationExcepiton"/> when the specified count is less than or equal to zero.
    /// </summary>
    /// <param name="count">
    /// The invalid number of books requested.
    /// </param>
    /// <returns>
    /// A task representing the asynchrounous test operation.
    /// </returns>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetTopExpensiveBooksAsync_ShouldThrowValidationException_WhenCountIsNotPositive(int count)
    {
        Func<Task> act = () => _bookService.GetTopExpensiveBooksAsync(count, true, cancellationToken);

        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Count must be a positive integer.");
        
        _bookRepositoryMock
            .Verify(repo => repo.GetTopExpensiveBooksAsync(
                It.IsAny<int>(), 
                It.IsAny<bool>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);

        _mapperMock
        .Verify(mapper => mapper.Map<IReadOnlyCollection<BookDto>>(
            It.IsAny<Book>()), 
        Times.Never);
    }

    #endregion of GetTopExpensiveBooksAsync Tests
    
    #endregion GET Methods Tests


    #region POST Methods Tests

    #region CreateBookAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.CreateBookAsync(BookCreateDto, CancellationToken)"/> successfully creates a new book
    /// and returns the mapped <see cref="BookCreateDto"/>.  
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task CreateBookAsync_ShouldCreateBook()

    {
        var book = CreateTestBook();
        var createBookDto = CreateTestBookCreateDto(book);

        _mapperMock
            .Setup(mapper => mapper.Map<Book>(createBookDto))
            .Returns(book);

        _bookRepositoryMock
            .Setup(repo => repo.AddBookAsync(book, cancellationToken))
            .ReturnsAsync(book);

        _mapperMock
            .Setup(mapper => mapper.Map<BookCreateDto>(book))
            .Returns(createBookDto);     

        var result = await _bookService.CreateBookAsync(createBookDto, cancellationToken);
        result.Should().BeEquivalentTo(createBookDto);

        _bookRepositoryMock
            .Verify(repo => repo.AddBookAsync(book, cancellationToken), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<BookCreateDto>(book), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<Book>(createBookDto), Times.Once);
    }

    /// <summary>
    /// Verifies <see cref="BookService.CreateBookAsync(BookCreateDto, CancellationToken)"/> maps the provided <see cref="BookCreateDto"/>
    /// to a <see cref="Book"/>, passes the mapped entity to the repository, and maps the created <see cref="Book"/> back to 
    /// <see cref="BookCreateDto"/>.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task CreateBookAsync_ShuldCallRepositoryWithMappedBook()
    {
        var book = CreateTestBook();
        var createBookDto = CreateTestBookCreateDto(book);

        _mapperMock
            .Setup(mapper => mapper.Map<Book>(createBookDto))
            .Returns(book);
        
        _bookRepositoryMock
            .Setup(repo => repo.AddBookAsync(book, cancellationToken))
            .ReturnsAsync(book);

        _mapperMock
            .Setup(mapper => mapper.Map<BookCreateDto>(book))
            .Returns(createBookDto);   

        await _bookService.CreateBookAsync(createBookDto, cancellationToken);
        
        _bookRepositoryMock
            .Verify(repo => repo.AddBookAsync(book, cancellationToken), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<BookCreateDto>(book), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<Book>(createBookDto), Times.Once);
    }

    #endregion CreateBookAsync Tests

    #endregion POST Methods Tests


    #region PUT Methods Tests

    #region UpdateBookAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookAsync(BookUpdateDto, CancellationToken)"/> updates an existing book and
    /// returns the updated <see cref="BookDto"/>.  
    /// </summary>
    /// <remarks>
    /// This test verifies that:
    /// <list type="bullet">
    /// <item><description>The existing book is retrieved from the repository</description></item>
    /// <item><description>The update DTO is mapped to a <see cref="Book"/> entity.</description></item>
    /// <item><description>The updated entity is passed to the repository.</description></item>
    /// <item><description>The updated entity is mapped back to a <see cref="BookDto"/>.</description></item>
    /// </list> 
    /// </ramrks>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task UpdateBookAsync_ShouldUpdateBook()
    {
        var book = CreateTestBook();
        var expectedResult = CreateTestBookDto(book);
        var bookUpdateDto = CreateTestBookUpdateDto(book);

         _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken))
            .ReturnsAsync(book);
        _mapperMock
            .Setup(mapper => mapper.Map<Book>(bookUpdateDto))
            .Returns(book);
        _bookRepositoryMock
            .Setup(repo => repo.UpdateBookAsync(book, cancellationToken))
            .ReturnsAsync(book);   
        _mapperMock
            .Setup(mapper => mapper.Map<BookDto>(book))
            .Returns(expectedResult);

        var result = await _bookService.UpdateBookAsync(bookUpdateDto, cancellationToken);
        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Once);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookAsync(book, cancellationToken), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<Book>(bookUpdateDto), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookAsync(BookUpdateDto, CancellationToken)"/> throws a <see cref="ValidationException"/>
    /// when the specified book ID is <see langword="null"/>, empty, or consists only of whitespace characters
    /// </summary>
    /// <param name="bookId">
    /// An invalid book identifier to validate.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task UpdateBookAsync_ShouldThrowValidationException_WhenIdIsNullOrWhitespace(string? bookId)
    {
        var book = CreateTestBook();
        book.Id = bookId;
        var bookUpdateDto = CreateTestBookUpdateDto(book);

        Func<Task> act = () => _bookService.UpdateBookAsync(bookUpdateDto, cancellationToken);

        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Book ID cannot be empty or null.");
        
        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Never);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookAsync(book, cancellationToken), Times.Never);
        _mapperMock
            .Verify(mapper => mapper.Map<Book>(bookUpdateDto), Times.Never);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookAsync(BookUpdateDto, CancellationToken)"/> throws a <see cref="ValidationException"/>
    /// when the specified book ID is not in valid ObjectId format.  
    /// </summary>
    /// <param name="bookId">
    /// An invalid book identifier with an incorrect ObjectId format.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("507f1f77bcf86cd7994390")]      //22 simboles
    [InlineData("not-an-object-id")]
    public async Task UpdateBookAsync_ShouldThrowValidationException_WhenIdIsInvalid(string bookId)
    {
        var book = CreateTestBook();
        book.Id = bookId;
        var bookUpdateDto = CreateTestBookUpdateDto(book);

        Func<Task> act = () => _bookService.UpdateBookAsync(bookUpdateDto, cancellationToken);

        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Invalid Book ID format.");
        
        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Never);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookAsync(book, cancellationToken), Times.Never);
        _mapperMock
            .Verify(mapper => mapper.Map<Book>(bookUpdateDto), Times.Never);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookAsync(BookUpdateDto, CancellationToken)"/> throws a 
    /// <see cref="NotFoundException"/> when the specified book does not exist. 
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task UpdateBookAsync_ShouldThrowNotFoundException_WhenBookDoesNotExists()
    {
        var book = CreateTestBook();

        _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken)).ReturnsAsync((Book?)null);

        var bookUpdateDto = CreateTestBookUpdateDto(book);

        Func<Task> act = () => _bookService.UpdateBookAsync(bookUpdateDto, cancellationToken);

        await act
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Book with ID '{book.Id}' not found.");

        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Once);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookAsync(book, cancellationToken), Times.Never);
        _mapperMock
            .Verify(mapper => mapper.Map<Book>(bookUpdateDto), Times.Never);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Never);

    }

    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookAsync(BookUpdateDto, CancellationToken)"/> throws an
    /// <see cref="InvalidOperationException"/> when the repository fails to update the book and returns <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// This test vefifies that:
    /// <list type="bullet">
    /// <item><description>The existing book is retrieved successfully.</description></item>
    /// <item><description>The update DTO is mapped to a <see cref="Book"/> entity.</description></item>
    /// <item><description>The repository update operation is invoked exactly once.</description></item>
    /// <item><description>An <see cref="InvalidOperationException"/> is thrown when the repository returns <see langword="null"/>.</description></item>
    /// <item><description>The updated entity is not mapped back to <see cref="BookDto"/> after the failure.</description></item>
    /// </list> 
    /// </remarks>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task UpdateBookAsync_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
    {
        var book = CreateTestBook();
        var bookUpdateDto = CreateTestBookUpdateDto(book);

         _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken))
            .ReturnsAsync(book);
        _mapperMock
            .Setup(mapper => mapper.Map<Book>(bookUpdateDto))
            .Returns(book);
        _bookRepositoryMock
            .Setup(repo => repo.UpdateBookAsync(book, cancellationToken))
            .ReturnsAsync((Book?)null!);  

        Func<Task> act = () => _bookService.UpdateBookAsync(bookUpdateDto, cancellationToken);
        await act
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Book update failed.");

        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Once);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookAsync(book, cancellationToken), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<Book>(bookUpdateDto), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Never);
    }

    #endregion UpdateBookAsync Tests

    #endregion PUT Methods Tests


    #region PATCH Methods Tests

    #region UpdateBookPartlyAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookPartlyAsync(BookUpdatePartlyDto, CancellationToken)"/> successfully updates
    /// a book when multiple valid fields are provided.
    /// </summary>
    /// <remarks>
    /// Ensures that:
    /// <list type="bullet">
    /// <item><description>The  service verifying that the book exists before updating it.</description></item>
    /// <item><description>The repository updates the book once.</description></item>
    /// <item><description>The updated entity is mapped to <see cref="BookDto"/>.</description></item>
    /// <item><description>The expected updated book is returned.</description></item>
    /// </list> 
    /// </remarks>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task UpdateBookPartlyAsync_ShouldUpdateBook()
    {
        var book = CreateTestBook();
        book.Title = "Clean Architecture";
        book.Annotation = "Updated annotation for testing.";
        book.Price = 99.99M;
        
        var bookUpdatePartlyDto = CreateTestBookUpdatePartlyDto(book);
        var expectedResult = CreateTestBookDto(book);

        _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken))
            .ReturnsAsync(book);
        _bookRepositoryMock
            .Setup(repo => repo.UpdateBookPartlyAsync(It.IsAny<List<UpdateDefinition<Book>>>(), book.Id!, cancellationToken))
            .ReturnsAsync(book);
        _mapperMock
            .Setup(mapper => mapper.Map<BookDto>(book))
            .Returns(expectedResult);

        var result = await _bookService.UpdateBookPartlyAsync(bookUpdatePartlyDto, cancellationToken);
        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Once);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookPartlyAsync(It.IsAny<List<UpdateDefinition<Book>>>(), book.Id!, cancellationToken), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookPartlyAsync(BookUpdatePartlyDto, CancellationToken)"/> throws a 
    /// <see cref="NotFoundException"/> when the specified book does not exist.
    /// </summary>
    /// <remarks>
    /// Ensures that:
    /// <list type="bullet">
    /// <item><description>The repository returns <see langword="null"/> when checking whether the book exists.</description></item>
    /// <item><description>A <see cref="NotFoundException"/> is thrown with th expected message.</description></item>
    /// <item><description>The partial update operaiton is never executed.</description></item>
    /// <item><description>The returned entity is not mapped to <see cref="BookDto"/>.</description></item>
    /// </list> 
    /// </remarks>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task UpdateBookPartlyAsync_ShouldThrowNotFoundException_WhenBookDoesNotExist()
    {
        var book = CreateTestBook();

        _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken))
            .ReturnsAsync((Book?)null);

        var updateBookPartlyDto = CreateTestBookUpdatePartlyDto(book);

        Func<Task> act = () => _bookService.UpdateBookPartlyAsync(updateBookPartlyDto, cancellationToken);

        await act
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Book with ID '{book.Id}' not found.");

        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Once);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookPartlyAsync(It.IsAny<List<UpdateDefinition<Book>>>(), book.Id!, cancellationToken), Times.Never);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookPartlyAsync(BookUpdatePartlyDto, CancellationToken)"/> throws a 
    /// <see cref="ValidationException"/> when no vaid fields are provided for the partial update.
    /// </summary>
    /// <remarks>
    /// Ensures that:
    /// <list type="bullet">
    /// <item><description>The repository checks that the book exists exactly once.</description></item>
    /// <item><description>A <see cref="ValidationException"/> is thrown with th expected message.</description></item>
    /// <item><description>The repository update method is never invoked.</description></item>
    /// <item><description>The updated entity is not mapped to <see cref="BookDto"/>.</description></item>
    /// </list> 
    /// </remarks>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task UpdateBookPartlyAsync_ShouldThrowValidationException_WhenNoValidFieldProvided()
    {
        var book = CreateTestBook();
        BookUpdatePartlyDto bookUpdatePartlyDto = new (book.Id!, "", [], 0, 0, "", "", [], new Uri("invalid", UriKind.Relative), null, "");

        _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken))
            .ReturnsAsync(book);

        Func<Task> act = () => _bookService.UpdateBookPartlyAsync(bookUpdatePartlyDto, cancellationToken);

        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("No valid fields provided for update.");
        
        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Once);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookPartlyAsync(It.IsAny<List<UpdateDefinition<Book>>>(), book.Id!, cancellationToken), Times.Never);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookPartlyAsync(BookUpdatePartlyDto, CancellationToken)"/> throws a 
    /// <see cref="ValidationException"/> when the provided book identifier is not a valid MongoDB ObjectId.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>A <see cref="ValidationException"/> thrown with expected message.</description></item>
    /// <item><description>The repository is never queried for the book.</description></item>
    /// <item><description>The repository update partly method is never invoked.</description></item>
    /// <item><description>The updated entity is not mapped to <see cref="BookDto"/>.</description></item>
    /// </list>
    /// </ramrks>
    /// <param name="bookId">
    /// The invalid book identifier used during the test.
    /// </param>
    /// <returns>
    /// A task reprsenting the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("507f1f77bcf86cd7994390")]      //22 simboles
    [InlineData("not-an-object-id")]
    public async Task UpdateBookPartlyAsync_ShouldThrowValidationException_WhenIdIsInvalid(string bookId)
    {
        var book = CreateTestBook();
        book.Id = bookId;
        var bookUpdatePartlyDto = CreateTestBookUpdatePartlyDto(book);

        Func<Task> act = () => _bookService.UpdateBookPartlyAsync(bookUpdatePartlyDto, cancellationToken);

        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Invalid Book ID format.");
        
        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Never);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookPartlyAsync(It.IsAny<List<UpdateDefinition<Book>>>(), book.Id!, cancellationToken), Times.Never);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookPartlyAsync(BookUpdatePartlyDto, CancellationToken)"/> throws a 
    /// <see cref="ValidationException"/> when the specified book ID is <see langword="null"/>, empty, or consists only of whitespace character
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>A <see cref="ValidationException"/> thrown with expected message.</description></item>
    /// <item><description>The repository is never queried for the book.</description></item>
    /// <item><description>The repository update partly method is never invoked.</description></item>
    /// <item><description>The updated entity is not mapped to <see cref="BookDto"/>.</description></item>
    /// </list>
    /// </ramrks>
    /// <param name="bookId">
    /// The invalid book identifier used during the test.
    /// </param>
    /// <returns>
    /// A task reprsenting the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task UpdateBookPartlyAsync_ShouldThrowValidationException_WhenIdIsNullOrWhiteSpace(string? bookId)
    {
        var book = CreateTestBook();
        book.Id = bookId;
        var bookUpdatePartlyDto = CreateTestBookUpdatePartlyDto(book);

        Func<Task> act = () => _bookService.UpdateBookPartlyAsync(bookUpdatePartlyDto, cancellationToken);

        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Book ID cannot be empty or null.");
        
        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Never);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookPartlyAsync(It.IsAny<List<UpdateDefinition<Book>>>(), book.Id!, cancellationToken), Times.Never);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Never);
    }
    
    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookPartlyAsync(BookUpdatePartlyDto, CancellationToken)"/> successfully updates
    /// only the book title when it is the only valid field provided.
    /// </summary>
    /// <remarks>
    /// Ensures that:
    /// <list type="bullet">
    /// <item><description>The  service verifying that the book exists before updating it.</description></item>
    /// <item><description>The repository updates the book once.</description></item>
    /// <item><description>The updated entity is mapped to <see cref="BookDto"/>.</description></item>
    /// <item><description>The expected updated book is returned.</description></item>
    /// </list> 
    /// </remarks>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task UpdateBookPartlyAsync_ShouldUpdateBook_WhenOnlyTitleToUpdate()
    {
        var book = CreateTestBook();
        book.Title = "New Title For Update Partly Tests";
        
        var bookUpdatePartlyDto = CreateTestBookUpdatePartlyDto(book);
        var expectedResult = CreateTestBookDto(book);

        _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken))
            .ReturnsAsync(book);
        _bookRepositoryMock
            .Setup(repo => repo.UpdateBookPartlyAsync(It.IsAny<List<UpdateDefinition<Book>>>(), book.Id!, cancellationToken))
            .ReturnsAsync(book);
        _mapperMock
            .Setup(mapper => mapper.Map<BookDto>(book))
            .Returns(expectedResult);

        var result = await _bookService.UpdateBookPartlyAsync(bookUpdatePartlyDto, cancellationToken);
        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Once);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookPartlyAsync(It.IsAny<List<UpdateDefinition<Book>>>(), book.Id!, cancellationToken), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.UpdateBookPartlyAsync(BookUpdatePartlyDto, CancellationToken)"/> successfully updates
    /// only the book link when it is the only valid field provided.
    /// </summary>
    /// <remarks>
    /// Ensures that:
    /// <list type="bullet">
    /// <item><description>The  service verifying that the book exists before updating it.</description></item>
    /// <item><description>The repository updates the book once.</description></item>
    /// <item><description>The updated entity is mapped to <see cref="BookDto"/>.</description></item>
    /// <item><description>The expected updated book is returned.</description></item>
    /// </list> 
    /// </remarks>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task UpdateBookPartlyAsync_ShouldUpdateBook_WhenOnlyLinkToUpdate()
    {
        var book = CreateTestBook();
        book.Link = new Uri("https://netbymarina.dev", UriKind.Absolute);
        
        var bookUpdatePartlyDto = CreateTestBookUpdatePartlyDto(book);
        var expectedResult = CreateTestBookDto(book);

        _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken))
            .ReturnsAsync(book);
        _bookRepositoryMock
            .Setup(repo => repo.UpdateBookPartlyAsync(It.IsAny<List<UpdateDefinition<Book>>>(), book.Id!, cancellationToken))
            .ReturnsAsync(book);
        _mapperMock
            .Setup(mapper => mapper.Map<BookDto>(book))
            .Returns(expectedResult);

        var result = await _bookService.UpdateBookPartlyAsync(bookUpdatePartlyDto, cancellationToken);
        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(book.Id!, cancellationToken), Times.Once);
        _bookRepositoryMock
            .Verify(repo => repo.UpdateBookPartlyAsync(It.IsAny<List<UpdateDefinition<Book>>>(), book.Id!, cancellationToken), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Once);
    }

    #endregion UpdateBookPartlyAsync Tests

    #endregion PATCH Methods Tests

    #region DELETE Methods Tests

    #region DeleteBookByIdAsync Tests

    /// <summary>
    /// Verifies that <see cref="BookService.DeleteBookByIdAsync(string, CancellationToken)"/>  deletes an existing book and returns
    /// the corresponding <see cref="BookDto"/>.  
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task DeleteBookByIdAsync_ShouldDeleteBook()
    {
        var book = CreateTestBook();
        var expectedResult = CreateTestBookDto(book);
        string bookId = book.Id!;

        _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(bookId, cancellationToken))
                .ReturnsAsync(book);
        _bookRepositoryMock
            .Setup(repo => repo.DeleteBookByIdAsync(bookId, cancellationToken))
            .ReturnsAsync(book);
        _mapperMock
            .Setup(mapper => mapper.Map<BookDto>(book))
            .Returns(expectedResult);

        var result = await _bookService.DeleteBookByIdAsync(bookId, cancellationToken);
        result.Should().BeEquivalentTo(expectedResult);

        _bookRepositoryMock
            .Verify(repo => repo.DeleteBookByIdAsync(bookId, cancellationToken), Times.Once);
        _mapperMock
            .Verify(mapper => mapper.Map<BookDto>(book), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.DeleteBookByIdAsync(string, CancellationToken)"/> throws a <see cref="ValidationException"/>
    /// when the specified book ID is <see langword="null"/>, empty, or consists only of whitespace characters.  
    /// </summary>
    /// <param name="bookId">
    /// An invalid book identifier to validate.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task DeleteBookByIdAsync_ShouldThrowValidationException_WhenIdIsNullOrWhitespace(string? bookId)
    {
        Func<Task> act = () => _bookService.DeleteBookByIdAsync(bookId!, cancellationToken);

        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Book ID cannot be empty or null.");

        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepositoryMock
            .Verify(repo => repo.DeleteBookByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(mapper => 
            mapper.Map<BookDto>(It.IsAny<Book>()), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.DeleteBookByIdAsync(string, CancellationToken)"/> throws a <see cref="ValidationException"/>
    /// when the specified book ID is not in valid ObjectId format.  
    /// </summary>
    /// <param name="bookId">
    /// An invalid book identifier with an incorrect ObjectId format.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("507f1f77bcf86cd7994390")]      //22 simboles
    [InlineData("not-an-object-id")]
    public async Task DeleteBookByIdAsync_ShouldThrowValidationException_WhenIdIsInvalid(string bookId)
    {
        Func<Task> act = () => _bookService.DeleteBookByIdAsync(bookId!, cancellationToken);

        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Invalid Book ID format.");

        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepositoryMock
            .Verify(repo => repo.DeleteBookByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(mapper => 
            mapper.Map<BookDto>(It.IsAny<Book>()), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="BookService.DeleteBookByIdAsync(string, CancellationToken)"/> throws a <see cref="NotFoundException"/>
    /// when the specified book does not exist. 
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// </returns>
    [Fact]
    public async Task DeleteBookByIdAsync_ShouldThrowNotFoundException_WhenBookDoesNotExists()
    {
        var bookId = ObjectId.GenerateNewId().ToString();
        
        _bookRepositoryMock
            .Setup(repo => repo.GetBookByIdAsync(bookId, cancellationToken)).ReturnsAsync((Book?)null);

        Func<Task> act = () => _bookService.DeleteBookByIdAsync(bookId, cancellationToken);

        await act
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Book with ID '{bookId}' not found.");

        _bookRepositoryMock
            .Verify(repo => repo.GetBookByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _bookRepositoryMock
            .Verify(repo => repo.DeleteBookByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(mapper => 
            mapper.Map<BookDto>(It.IsAny<Book>()), Times.Never);
    }

    #endregion DeleteBookByIdAsync Tests

    #endregion DELETE Methods Tests

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

/// <summary>
/// Creates a collection of test books and the corresponding DTOs based on the specified availability filter.
/// </summary>
/// <param name="count">
/// The maximum number of DTOs to include in the expected result.
/// </param>
/// <param name="isAvailable">
/// The availability filter used to detemine which books are included in the expected DTO colleciton.
/// If <see langword="null"/>, books of any availability status are included. 
/// </param>
/// <returns>
/// A tuple containing the generated collection of <see cref="Book"/> entities and the corresponding expected collection
/// of <see cref="BookDto"/> objects.
/// </returns>
    private static (List<Book> Books, List<BookDto> ExpectedResult) CreateBooksAndDtos(int count, bool? isAvailable)
    {
        List<Book> books = [];
        List<BookDto> expectedResult = [];

        for(int i = 0; i < count * 2; i++)
        {
            var book = CreateTestBook();
            book.IsAvailable = (i % 2) == 0;

            books.Add(book);

            if(expectedResult.Count < count && (!isAvailable.HasValue || book.IsAvailable == isAvailable))
            {
                expectedResult.Add(CreateTestBookDto(book));
            }
        }
        return (books, expectedResult);
    }

    /// <summary>
    /// Creates a <see cref="BookCreateDto"/> populated with the values from the specified <see cref="Book"/> instance.  
    /// </summary>
    /// <param name="book">
    /// The source <see cref="Book"/> used to populate DTO. 
    /// </param>
    /// <returns>
    /// A new <see cref="BookCreateDto"/> containing the values from the specified book. 
    /// </returns>
    private static BookCreateDto CreateTestBookCreateDto(Book book)
    {
        return new BookCreateDto(
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
    /// Creates a <see cref="BookUpdateDto"/> populated with the values from the specified <see cref="Book"/> instance.  
    /// </summary>
    /// <param name="book">
    /// The source <see cref="Book"/> used to populate DTO. 
    /// </param>
    /// <returns>
    /// A <see cref="BookUpdateDto"/> containing the same values as the source book. 
    /// </returns>
    private static BookUpdateDto CreateTestBookUpdateDto(Book book)
    {
        return new (
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
    /// Creates a <see cref="BookUpdatePartlyDto"/> populated with the values from the specified <see cref="Book"/> instance.  
    /// </summary>
    /// <param name="book">
    /// The source <see cref="Book"/> used to populate DTO. 
    /// </param>
    /// <returns>
    /// A <see cref="BookUpdatePartlyDto"/> containing the same values as the source book. 
    /// </returns>
    private static BookUpdatePartlyDto CreateTestBookUpdatePartlyDto(Book book)
    {
        return new (
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

    #endregion Private Helper Methods
}