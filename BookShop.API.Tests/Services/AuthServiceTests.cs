using AutoMapper;
using BookShop.API.DTOs.Auth;
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

    /// <summary>
    /// Vefifies that <see cref="AuthServices.RegisterUserAsync(UserRegisterDto, CancellationToken)"/> successfully creates a new user,
    /// assigns the default role, hashes the password, sends an email confirmation link, and returns the identifier of the created user.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RegisterUserAsync_ShouldCreateUser_WhenRegistrationDataIsValid()
    {
        UserRegisterDto registerDto = TestValidRegisterData();
        const int expectedUserId = 10;
        User? savedUser = null;

        // User with the specified username and email does not exist
        SetupUserDoesNotExist(registerDto, null, cancellationToken);
        // Role is exists
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
        // Email Sending
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
    /// Creates valid registration data for unit tests.
    /// </summary>
    /// <returns>
    /// A <see cref="UserRegisterDto"/> containing valid username, email address, and password.
    /// </returns>
    private static UserRegisterDto TestValidRegisterData()
    {
        return new ("userName", "userEmail@email.com", "P@ssw0rd123");
    }

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
    /// Verifies that the created user contains the expected values after a successful registration operation.
    /// </summary>
    /// <param name="savedUser">
    /// The user entity created by <see cref="AuthService.RegisterUserAsync(UserRegisterDto, CancellationToken)"/> or by
    /// <see cref="AuthService.RegisterAdminUserAsync(UserRegisterDto, CancellationToken)"/>.  
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
}