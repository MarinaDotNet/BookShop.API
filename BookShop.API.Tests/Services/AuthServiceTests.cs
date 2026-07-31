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
        _userRepositoryMock.Setup(repo => 
            repo.GetUserByNormalizedUsernameAsync(NormalizeString(registerDto.Username), cancellationToken))
            .ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(repo => 
            repo.GetUserByNormalizedEmailAsync(NormalizeString(registerDto.Email), cancellationToken))
            .ReturnsAsync((User?)null);

        // Role is exists
        _userRepositoryMock.Setup(repo =>
            repo.GetRoleByNameAsync(userRole.Name, cancellationToken))
            .ReturnsAsync(userRole);

        // Password hashing
         _passwordHasherMock.Setup(hasher => 
            hasher.HashPassword(It.IsAny<User>(), registerDto.Password))
            .Returns("hashed-password");

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
        _authTokenServiceMock.Setup(service => 
            service.CreateToken(
                AuthTokens.AuthTokenPurpose.EmailConfirmation,
                expectedUserId, 
                It.IsAny<DateTime>()))
            .Returns("token");
        
        // Link creation
        _authLinkGeneratorMock.Setup(generator => 
            generator.CreateEmailConfirmationLink("token"))
            .Returns(new Uri("https://localhost/confirm"));

        // Email Sending
        _emailSenderMock.Setup(sender => 
            sender.SendEmailConfirmationAsync(
                registerDto.Email, 
                It.IsAny<Uri>(), 
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        int id = await _authService.RegisterUserAsync(registerDto, cancellationToken);

        // Assert
        id.Should().Be(expectedUserId);

        Assert.NotNull(savedUser);

        Assert.Equal(registerDto.Username, savedUser.UserName);
        Assert.Equal(NormalizeString(registerDto.Username), savedUser.NormalizedUsername);

        Assert.Equal(registerDto.Email, savedUser.Email);
        Assert.Equal(NormalizeString(registerDto.Email), savedUser.NormalizedEmail);

        Assert.Equal("hashed-password", savedUser.PasswordHash);

        Assert.Single(savedUser.UserRoles);
        Assert.Equal(userRole.Id, savedUser.UserRoles.First().RoleId);

        Assert.True(savedUser.IsActive);
        Assert.False(savedUser.IsDeleted);
        Assert.False(savedUser.IsEmailConfirmed);
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
}