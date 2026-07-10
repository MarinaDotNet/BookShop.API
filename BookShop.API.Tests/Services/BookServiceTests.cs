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

    #endregion Private Helper Methods
}