using AutoMapper;
using BookShop.API.DTOs.Auth;
using BookShop.API.Exceptions;
using BookShop.API.Models.Auth;
using BookShop.API.Repositories;
using BookShop.API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BookShop.API.Tests.Services;

/// <summary>
/// Contains unit tests for <see cref="AuthServices"/>. 
/// </summary>
/// <remarks>
/// Verifies authentication-related operations such as user registration, administration registration, login, email confirmation,
/// refresh token handling, account recovery, and account management.
/// </remarks>
public class AuthServiceTests
{
    private readonly AuthServices _authService;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly Mock<IAuthTokenService> _authTokenServiceMock;
    private readonly Mock<IAuthLinkGenerator> _authLinkGeneratorMock;
    private readonly Mock<IAuthEmailSender> _emailSenderMock;
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock;
    private readonly Mock<IRefreshTokenHasher> _refreshTokenHasherMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private readonly Role adminRole = new(){Id = 1, Name = "admin"};
    private readonly Role userRole = new(){Id = 2, Name = "user"};

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher<User>>();
        _authTokenServiceMock = new Mock<IAuthTokenService>();
        _authLinkGeneratorMock = new Mock<IAuthLinkGenerator>();
        _emailSenderMock = new Mock<IAuthEmailSender>();
        _refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();
        _refreshTokenHasherMock = new Mock<IRefreshTokenHasher>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _mapperMock = new Mock<IMapper>();

        _authService = new AuthServices(
            _userRepositoryMock.Object, 
            _passwordHasherMock.Object, 
            _authTokenServiceMock.Object,
            _authLinkGeneratorMock.Object,
            _emailSenderMock.Object,
            _refreshTokenGeneratorMock.Object,
            _refreshTokenHasherMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);
    }

    #region Registration & Login Tests

    #region of RegisterUserAsync Tests

    /// <summary>
    /// Verifies that <see cref="AuthServices.RegisterUserAsync(UserRegisterDto, CancellationToken)"/> successfully creates a new user,
    /// assigns the default role, hashes the password, sends an email confirmation link, and returns the identifier of the created user.
    /// </summary>
    [Fact]
    public async Task RegisterUserAsync_ShouldCreateUser_WhenRegistrationDataIsValid()
    {
        UserRegisterDto registerDto = TestValidRegisterData();
        const int expectedUserId = 10;
        User? savedUser = null;

        // User with the specified username and email does not exist
        SetupUserDoesNotExist(registerDto, null, cancellationToken);
        // Role exists
        SetupUserRoleExists(userRole.Name, userRole, cancellationToken);
        // Password hashing
        SetupPasswordHashing(registerDto.Password, "hashed-password");

        // Saving the user
        _userRepositoryMock.Setup(repo => 
            repo.AddUserAsync(It.IsAny<User>(), cancellationToken))
            .Callback<User, CancellationToken>((u, _) => savedUser = u)
            .ReturnsAsync((User u, CancellationToken _) =>
            {
                u.Id = expectedUserId;
                return u;
            });

        // Token creation
        SetupTokenGeneration(AuthTokens.AuthTokenPurpose.EmailConfirmation, expectedUserId, "token"); 
        // Link creation
        SetupLinkGeneration("token", new Uri("https://localhost/confirm"));
        // Email sending
        SetupEmailSending(registerDto.Email, cancellationToken);

        // Act
        int id = await _authService.RegisterUserAsync(registerDto, cancellationToken);

        // Assert
        id.Should().Be(expectedUserId);

        Assert.NotNull(savedUser);

        AssertCreatedUser(savedUser!, registerDto, "hashed-password");

        Assert.Single(savedUser.UserRoles);
        Assert.Equal(userRole.Id, savedUser.UserRoles.First().RoleId);

        Assert.True(savedUser.CreatedAt <= DateTime.UtcNow);
        Assert.True(savedUser.UpdatedAt <= DateTime.UtcNow);

        _passwordHasherMock.Verify(hasher =>
            hasher.HashPassword(It.IsAny<User>(), registerDto.Password), Times.Once);

        _userRepositoryMock.Verify(repo =>
            repo.AddUserAsync(It.Is<User>(u => 
                u.UserName == registerDto.Username 
                && u.Email == registerDto.Email), cancellationToken), 
            Times.Once);
        
        _userRepositoryMock.Verify(repo => 
            repo.UpdateUserAsync(It.Is<User>(u => u.EmailConfirmationSentAt.HasValue), cancellationToken), Times.Once);

        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                registerDto.Email, 
                It.IsAny<Uri>(), 
                cancellationToken), 
            Times.Once);
    }

    /// <summary>
    /// Verifies that registration fails when the specified username is already used by another user account. 
    /// </summary>
    [Fact]
    public async Task RegisterUserAsync_ShouldThrowConflictException_WhenUsernameAlreadyExists()
    {
        var userRegisterDto = TestValidRegisterData();

        _userRepositoryMock.Setup(repo => 
                repo.GetUserByNormalizedUsernameAsync(
                    NormalizeString(userRegisterDto.Username), 
                    cancellationToken))
            .ReturnsAsync(new User { UserName = userRegisterDto.Username });

        Func<Task> act = () => _authService.RegisterUserAsync(userRegisterDto, cancellationToken);

        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("Username is already taken.");

        _userRepositoryMock.Verify(repo => 
            repo.AddUserAsync(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
        
        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                It.IsAny<string>(), 
                It.IsAny<Uri>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
        
        _passwordHasherMock.Verify(hasher =>
            hasher.HashPassword(
                It.IsAny<User>(), 
                It.IsAny<string>()), 
            Times.Never);     
    }

    /// <summary>
    /// Verifies that registration fails when the specified email is already used by another user account. 
    /// </summary>
    [Fact]
    public async Task RegisterUserAsync_ShouldThrowConflictException_WhenEmailAlreadyExists()
    {
        var userRegisterDto = TestValidRegisterData();

        _userRepositoryMock.Setup(repo => 
                repo.GetUserByNormalizedEmailAsync(
                    NormalizeString(userRegisterDto.Email), 
                    cancellationToken))
            .ReturnsAsync(new User { Email = userRegisterDto.Email });

        Func<Task> act = () => _authService.RegisterUserAsync(userRegisterDto, cancellationToken);

        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("Email is already taken.");

        _userRepositoryMock.Verify(repo => 
            repo.AddUserAsync(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>()), 
                Times.Never);
        
        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                It.IsAny<string>(), 
                It.IsAny<Uri>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
        
        _passwordHasherMock.Verify(hasher =>
            hasher.HashPassword(
                It.IsAny<User>(), 
                It.IsAny<string>()), 
            Times.Never);     
    }

    /// <summary>
    /// Verifies that registration fails when the specified role does not exist in the system.
    /// </summary>
    [Fact]
    public async Task RegisterUserAsync_ShouldThrowInvalidOperationException_WhenRoleDoesNotExist()
    {
        var userRegisterDto = TestValidRegisterData();
        SetupUserDoesNotExist(userRegisterDto, null, cancellationToken);
        string roleName = "user";

        _userRepositoryMock.Setup(repo =>
            repo.GetRoleByNameAsync(roleName, cancellationToken))
        .ReturnsAsync((Role?)null);

        Func<Task> act = () => _authService.RegisterUserAsync(userRegisterDto, cancellationToken);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Role '{roleName}' not found.");
        
        _userRepositoryMock.Verify(repo => 
            repo.GetUserByNormalizedEmailAsync(
                NormalizeString(userRegisterDto.Email), 
                cancellationToken), 
            Times.Once);
        _userRepositoryMock.Verify(repo => 
            repo.GetUserByNormalizedUsernameAsync(
                NormalizeString(userRegisterDto.Username), 
                cancellationToken), 
            Times.Once);
        _userRepositoryMock.Verify(repo =>
            repo.GetRoleByNameAsync(
                roleName, 
                cancellationToken), 
            Times.Once);
        
        _userRepositoryMock.Verify(repo => 
            repo.AddUserAsync(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);

        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                It.IsAny<string>(), 
                It.IsAny<Uri>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="AuthServices.RegisterUserAsync(UserRegisterDto, CancellationToken)"/> fails when the supplied
    /// registration data is invalid. 
    /// </summary>
    /// <param name="invalidRegisterDto">
    /// Invalid registration data passed to the authentication service.
    /// </param>
    [Theory]
    [MemberData(nameof(InvalidRegisterData))]
    public async Task RegisterUserAsync_ShouldThrowArgumentException_WhenRegistrationDataIsInvalid(UserRegisterDto invalidRegisterDto)
    {

        // User with the specified username and email does not exist
        SetupUserDoesNotExist(invalidRegisterDto, null, cancellationToken);
        // Role is exists
        SetupUserRoleExists(userRole.Name, userRole, cancellationToken);

        Func<Task> act = () => _authService.RegisterUserAsync(invalidRegisterDto, cancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
        
        _userRepositoryMock.Verify(repo =>
            repo.AddUserAsync(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
        
        _passwordHasherMock.Verify(hasher =>
            hasher.HashPassword(
                It.IsAny<User>(), 
                It.IsAny<string>()), 
            Times.Never);

        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                It.IsAny<string>(), 
                It.IsAny<Uri>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    /// <summary>
    /// Verifies that registration fails when the registration data object is <see langword="null"/>. 
    /// </summary>
    [Fact]
    public async Task RegisterUserAsync_ShouldThrowArgumentNullException_WhenRegisterDtoIsNull()
    {
        UserRegisterDto? registerDto = null;

        Func<Task> act = () => _authService.RegisterUserAsync(registerDto!, cancellationToken);

        await act.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName("userRegisterDto");

        _userRepositoryMock.Verify(repo =>
            repo.AddUserAsync(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);

        _passwordHasherMock.Verify(hasher =>
            hasher.HashPassword(
                It.IsAny<User>(), 
                It.IsAny<string>()), 
            Times.Never);

        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                It.IsAny<string>(), 
                It.IsAny<Uri>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    #endregion of RegisterUserAsync Tests

    #region of RegisterAdminAsync Tests

    /// <summary>
    /// Verifies that <see cref="AuthServices.RegisterAdminAsync(UserRegisterDto, CancellationToken)"/> successfully creates a new 
    /// administrator account, assigns the admin role, hashes the password, sends an email confirmation link, and returns the 
    /// identifier of the created administrator account.
    /// </summary>
    [Fact]
    public async Task RegisterAdminAsync_ShouldCreateAdminUser_WhenRegistrationDataIsValid()
    {
        UserRegisterDto registerDto = TestValidRegisterData();
        const int expectedUserId = 20;
        User? savedUser = null;

        SetupUserDoesNotExist(registerDto, null, cancellationToken);
        SetupUserRoleExists(adminRole.Name, adminRole, cancellationToken);
        SetupPasswordHashing(registerDto.Password, "hashed-admin-password");

        _userRepositoryMock.Setup(repo => 
                repo.AddUserAsync(It.IsAny<User>(), cancellationToken))
            .Callback<User, CancellationToken>((u, _) => savedUser = u)
            .ReturnsAsync((User u, CancellationToken _) =>
            {
                u.Id = expectedUserId;
                return u;
            });

        SetupTokenGeneration(AuthTokens.AuthTokenPurpose.EmailConfirmation, expectedUserId, "admin-token");
        SetupLinkGeneration("admin-token", new Uri("https://localhost/confirm"));
        SetupEmailSending(registerDto.Email, cancellationToken);

        int id = await _authService.RegisterAdminAsync(registerDto, cancellationToken);

        id.Should().Be(expectedUserId);

        Assert.NotNull(savedUser);

        AssertCreatedUser(savedUser!, registerDto, "hashed-admin-password");

        Assert.Single(savedUser.UserRoles);
        Assert.Equal(adminRole.Id, savedUser.UserRoles.First().RoleId);

        Assert.True(savedUser.CreatedAt <= DateTime.UtcNow);
        Assert.DoesNotContain(savedUser.UserRoles, ur => ur.RoleId == userRole.Id);
        Assert.True(savedUser.UpdatedAt <= DateTime.UtcNow);

        _passwordHasherMock.Verify(hasher =>
            hasher.HashPassword(
                It.IsAny<User>(), 
                registerDto.Password), 
            Times.Once);

        _userRepositoryMock.Verify(repo => 
            repo.AddUserAsync(
                It.Is<User>(u => 
                    u.UserName == registerDto.Username
                    && u.Email == registerDto.Email
                    && u.UserRoles.Any(ur => ur.RoleId == adminRole.Id)), 
                cancellationToken),
            Times.Once);
        _userRepositoryMock.Verify(repo =>
            repo.UpdateUserAsync(
                It.Is<User>(u => u.EmailConfirmationSentAt.HasValue),
                cancellationToken),
            Times.Once);

        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                registerDto.Email, 
                It.IsAny<Uri>(), 
                cancellationToken), 
            Times.Once);
    }

    /// <summary>
    /// Verifies that registration fails when the specified username is already used by another user account.
    /// </summary>
     [Fact]
    public async Task RegisterAdminAsync_ShouldThrowConflictException_WhenUsernameAlreadyExists()
    {
        var registerDto = TestValidRegisterData();

        _userRepositoryMock.Setup(repo => 
                repo.GetUserByNormalizedUsernameAsync(
                    NormalizeString(registerDto.Username), 
                    cancellationToken))
            .ReturnsAsync(new User { UserName = registerDto.Username });

        Func<Task> act = () => _authService.RegisterAdminAsync(registerDto, cancellationToken);

        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("Username is already taken.");

        _userRepositoryMock.Verify(repo => 
            repo.AddUserAsync(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
        
        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                It.IsAny<string>(), 
                It.IsAny<Uri>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
        
        _passwordHasherMock.Verify(hasher =>
            hasher.HashPassword(
                It.IsAny<User>(), 
                It.IsAny<string>()), 
            Times.Never);     
    }
    
    /// <summary>
    /// Verifies that registration fails when the specified email is already used by another user account. 
    /// </summary>
    [Fact]
    public async Task RegisterAdminAsync_ShouldThrowConflictException_WhenEmailAlreadyExists()
    {
        var registerDto = TestValidRegisterData();

        _userRepositoryMock.Setup(repo => 
                repo.GetUserByNormalizedEmailAsync(
                    NormalizeString(registerDto.Email), 
                    cancellationToken))
            .ReturnsAsync(new User { Email = registerDto.Email });

        Func<Task> act = () => _authService.RegisterAdminAsync(registerDto, cancellationToken);

        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("Email is already taken.");

        _userRepositoryMock.Verify(repo => 
            repo.AddUserAsync(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>()), 
                Times.Never);
        
        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                It.IsAny<string>(), 
                It.IsAny<Uri>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
        
        _passwordHasherMock.Verify(hasher =>
            hasher.HashPassword(
                It.IsAny<User>(), 
                It.IsAny<string>()), 
            Times.Never);     
    }

    /// <summary>
    /// Verifies that registration fails when the admin role does not exist in the system.
    /// </summary>
    [Fact]
    public async Task RegisterAdminAsync_ShouldThrowInvalidOperationException_WhenAdminRoleDoesNotExist()
    {
        var registerDto = TestValidRegisterData();
        SetupUserDoesNotExist(registerDto, null, cancellationToken);

        _userRepositoryMock.Setup(repo =>
            repo.GetRoleByNameAsync(adminRole.Name, cancellationToken))
        .ReturnsAsync((Role?)null);

        Func<Task> act = () => _authService.RegisterAdminAsync(registerDto, cancellationToken);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Role '{adminRole.Name}' not found.");
        
        _userRepositoryMock.Verify(repo => 
            repo.GetUserByNormalizedEmailAsync(
                NormalizeString(registerDto.Email), 
                cancellationToken), 
            Times.Once);
        _userRepositoryMock.Verify(repo => 
            repo.GetUserByNormalizedUsernameAsync(
                NormalizeString(registerDto.Username), 
                cancellationToken), 
            Times.Once);
        _userRepositoryMock.Verify(repo =>
            repo.GetRoleByNameAsync(
                adminRole.Name, 
                cancellationToken), 
            Times.Once);
        
        _userRepositoryMock.Verify(repo => 
            repo.AddUserAsync(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);

        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                It.IsAny<string>(), 
                It.IsAny<Uri>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="AuthServices.RegisterAdminAsync(UserRegisterDto, CancellationToken)"/> fails when the supplied
    /// registration data is invalid. 
    /// </summary>
    /// <param name="invalidRegisterDto">
    /// Invalid registration data passed to the authentication service.
    /// </param>
    [Theory]
    [MemberData(nameof(InvalidRegisterData))]
    public async Task RegisterAdminAsync_ShouldThrowArgumentException_WhenRegistrationDataIsInvalid(UserRegisterDto invalidRegisterDto)
    {
        // User with the specified username and email does not exist
        SetupUserDoesNotExist(invalidRegisterDto, null, cancellationToken);
        // Role is exists
        SetupUserRoleExists(adminRole.Name, adminRole, cancellationToken);

        Func<Task> act = () => _authService.RegisterAdminAsync(invalidRegisterDto, cancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
        
        _userRepositoryMock.Verify(repo =>
            repo.AddUserAsync(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
        
        _passwordHasherMock.Verify(hasher =>
            hasher.HashPassword(
                It.IsAny<User>(), 
                It.IsAny<string>()), 
            Times.Never);

        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                It.IsAny<string>(), 
                It.IsAny<Uri>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    /// <summary>
    /// Verifies that registration fails when the registration data object is <see langword="null"/>. 
    /// </summary>
    [Fact]
    public async Task RegisterAdminAsync_ShouldThrowArgumentNullException_WhenRegisterDtoIsNull()
    {
        UserRegisterDto? registerDto = null;

        Func<Task> act = () => _authService.RegisterAdminAsync(registerDto!, cancellationToken);

        await act.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName("userRegisterDto");

        _userRepositoryMock.Verify(repo =>
            repo.AddUserAsync(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);

        _passwordHasherMock.Verify(hasher =>
            hasher.HashPassword(
                It.IsAny<User>(), 
                It.IsAny<string>()), 
            Times.Never);

        _emailSenderMock.Verify(sender =>
            sender.SendEmailConfirmationAsync(
                It.IsAny<string>(), 
                It.IsAny<Uri>(), 
                It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    #endregion of RegisterAdminAsync Tests

    #endregion Registration & Login Tests

    #region of Helper Methods

    #region of Test Data

    /// <summary>
    /// Contains invalid registration data for unit tests.
    /// </summary>
    public static IEnumerable<object?[]> InvalidRegisterData =>
    [
        [ new UserRegisterDto("", "", "") ],
        [ new UserRegisterDto(string.Empty, string.Empty, string.Empty) ],
        [ new UserRegisterDto("   ", "   ", "   ") ],
        [ new UserRegisterDto(" ", " ", " ") ],
        [ new UserRegisterDto("\t", "\t", "\t") ],
        [ new UserRegisterDto("", "userEmail@email.com", "P@ssw0rd123") ],
        [ new UserRegisterDto("userName", "", "P@ssw0rd123") ],
        [ new UserRegisterDto("userName", "userEmail@email.com", "")]
    ];
    
    /// <summary>
    /// Creates valid registration data for unit tests.
    /// </summary>
    /// <returns>
    /// A <see cref="UserRegisterDto"/> containing valid username, email address, and password.
    /// </returns>
    private static UserRegisterDto TestValidRegisterData()
    {
        return new ("userName", "userEmail@email.com", "P@ssw0rd123");
    }

    #endregion of Test Data

    #region of Mock Setup and Verification

    /// <summary>
    /// Normalizes the string using same rules as the authentication service.
    /// </summary>
    /// <param name="toNormalize">
    /// The value to normalize.
    /// </param>
    /// <returns>
    /// A trimmed uppercase string.
    /// </returns>
    private static string NormalizeString(string toNormalize) =>
        toNormalize.Trim().ToUpperInvariant();

    /// <summary>
    /// Configures the user repository mocks to simulate that no user exists with the specified username or email address.
    /// </summary>
    /// <param name="registerDto">
    /// Registration data containing the username and email to check.
    /// </param>
    /// <param name="userToReturn">
    /// The user instance to return by the repository methods. Pass <see langword="null"/> to simulate that no matching user exists.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token to be used for the repository method calls.
    /// </param>
    private void SetupUserDoesNotExist(
        UserRegisterDto registerDto, 
        User? userToReturn = null, 
        CancellationToken cancellationToken = default)
    {
        _userRepositoryMock.Setup(repo => 
            repo.GetUserByNormalizedUsernameAsync(NormalizeString(registerDto.Username), cancellationToken))
            .ReturnsAsync(userToReturn);
        _userRepositoryMock.Setup(repo => 
            repo.GetUserByNormalizedEmailAsync(NormalizeString(registerDto.Email), cancellationToken))
            .ReturnsAsync(userToReturn);
    }

    /// <summary>
    /// Configures the repository mock to return the specified role.
    /// </summary>
    /// <param name="roleName">
    /// Name of the role requested by the service.
    /// </param>
    /// <param name="expectedRole">
    /// The role instance returned by the repository.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token to be used for the repository method calls.
    /// </param>
    private void SetupUserRoleExists(string roleName, Role expectedRole, CancellationToken cancellationToken) => 
        _userRepositoryMock.Setup(repo =>
            repo.GetRoleByNameAsync(roleName, cancellationToken))
        .ReturnsAsync(expectedRole);

    /// <summary>
    /// Configures the password hasher mock to return the specified password hash.
    /// </summary>
    /// <param name="password">
    /// Plaintext password expected by the hasher.
    /// </param>
    /// <param name="shouldReturn">
    /// Hash value returned by the mock password hasher. 
    /// </param>
    private void SetupPasswordHashing(string password, string shouldReturn) => 
        _passwordHasherMock.Setup(hasher => 
            hasher.HashPassword(It.IsAny<User>(), password))
        .Returns(shouldReturn);

    /// <summary>
    /// Configures the authentication token service to return the specified token.
    /// </summary>
    /// <param name="purpose">
    /// Purpose of the authentication token.
    /// </param>
    /// <param name="userId">
    /// Identifier of the user for whom the token is generated.
    /// </param>
    /// <param name="token">
    /// Token value returned by the mocked service.
    /// </param>
    private void SetupTokenGeneration(AuthTokens.AuthTokenPurpose purpose, int userId, string token) => 
        _authTokenServiceMock.Setup(service => 
            service.CreateToken(
                purpose,
                userId, 
                It.IsAny<DateTime>()))
        .Returns(token);

    /// <summary>
    /// Configures the authentication link generator to return the specified confirmation link.
    /// </summary>
    /// <param name="token">
    /// Authentication token used to generate the confirmation link.
    /// </param>
    /// <param name="confirmationLink">
    /// Link returned by the mocked generator.
    /// </param>
    private void SetupLinkGeneration(string token, Uri confirmationLink) =>
        _authLinkGeneratorMock.Setup(generator => 
            generator.CreateEmailConfirmationLink(token))
        .Returns(confirmationLink);

    /// <summary>
    /// Configures the email sender mock to simulate successful email delivery.
    /// </summary>
    /// <param name="email">
    /// Recipient email address.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token to be used for the email sending operation.
    /// </param>
    private void SetupEmailSending(string email, CancellationToken cancellationToken) =>
        _emailSenderMock.Setup(sender => 
            sender.SendEmailConfirmationAsync(
                email, 
                It.IsAny<Uri>(), 
                cancellationToken))
        .Returns(Task.CompletedTask);

    /// <summary>
    /// Verifies that the created user entity contains the expected values after a successful registration.
    /// </summary>
    /// <param name="savedUser">
    /// The user entity captured from the repository mock.  
    /// </param>
    /// <param name="registerDto">
    /// The registration data used to create the user.
    /// </param>
    /// <param name="expectedPasswordHash">
    /// The expected hashed password value returned by the mocked password hasher.
    /// </param>
    private static void AssertCreatedUser(User savedUser, UserRegisterDto registerDto, string expectedPasswordHash)
    {
        Assert.Equal(registerDto.Username, savedUser.UserName);
        Assert.Equal(NormalizeString(registerDto.Username), savedUser.NormalizedUsername);

        Assert.Equal(registerDto.Email, savedUser.Email);
        Assert.Equal(NormalizeString(registerDto.Email), savedUser.NormalizedEmail);

        Assert.Equal(expectedPasswordHash, savedUser.PasswordHash);

        Assert.True(savedUser.IsActive);
        Assert.False(savedUser.IsDeleted);
        Assert.False(savedUser.IsEmailConfirmed);
    }

    #endregion of Mock Setup and Verification

    #endregion of Helper Methods
}