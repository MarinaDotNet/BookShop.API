using BookShop.API.DTOs.Auth;
using BookShop.API.Exceptions;
using BookShop.API.Models.Auth;
using BookShop.API.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticAssets;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using static BookShop.API.Models.AuthTokens;

namespace BookShop.API.Services;

/// <summary>
/// Provide authentication-related operations such as user registration, user login, user update.
/// </summary>
/// <remarks>
/// This service handles user operations by validating input data, checking for existing users, hashing passwords, and assigning default roles.
/// </remarks>
/// <param name="userRepository">
/// An instance of <see cref="IUserRepository"/> for accessing user data storage and retrieval operations.
/// </param>
/// <param name="passwordHasher">
/// The password hasher used to securely hash user passwords before storing them in the database.
/// </param>
/// <param name="authLinkGenerator">
/// The service responsible for generating authentication-related links, such as email confirmation links.
/// </param>
/// <param name="authTokenService">
/// The service used to create and manage authentication tokens for various purposes, such as email confirmation.
/// </param>
/// <param name="emailSender">
/// The service responsible for sending authentication-related emails, such as email confirmation messages.
/// </param>
public class AuthServices(
    IUserRepository userRepository, 
    IPasswordHasher<User> passwordHasher, 
    IAuthTokenService authTokenService, 
    IAuthLinkGenerator authLinkGenerator,
    IAuthEmailSender emailSender,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher,
    IJwtTokenService jwtTokenService
    ) 
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly IAuthTokenService _authTokenService = authTokenService;
    private readonly IAuthLinkGenerator _authLinkGenerator = authLinkGenerator;
    private readonly IAuthEmailSender _emailSender = emailSender;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = refreshTokenGenerator;
    private readonly IRefreshTokenHasher _refreshTokenHasher = refreshTokenHasher;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    /// <summary>
    /// Registers a new user asynchronously using the specified registration details.
    /// </summary>
    /// <param name="userRegisterDto">An object containing the user's registration information, including username, email, and password. Cannot be
    /// null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the registration operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifier of the newly
    /// registered user.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a user with the provided username or email already exists, or if the default user role cannot be
    /// found.</exception>
    public async Task<int> RegisterUserAsync(UserRegisterDto userRegisterDto, CancellationToken cancellationToken)
    {
        //Validate DTO
        ValidateRegistrationInput(userRegisterDto);

        //Check if user exists
        var normalizedUsername = NormalizeInput(userRegisterDto.Username);
        var normalizedEmail = NormalizeInput(userRegisterDto.Email);
        await EnsureUserDoesNotExists(normalizedUsername, normalizedEmail, cancellationToken);

        //Create User
        var user = CreateUserEntity(userRegisterDto, normalizedUsername, normalizedEmail);

        //Assign Role
        await AssignRoleAsync(user, "user", cancellationToken);

        //Save user
        var userCreated = await _userRepository.AddUserAsync(user, cancellationToken);

        await SendEmailConfirmationLinkAsync(userCreated, cancellationToken);

        return userCreated.Id;
    }

    /// <summary>
    /// Confirms a user's email address using an email confirmation token.
    /// </summary>
    /// <remarks>
    /// This method validated the provided token for the <see cref="AuthTokenPurpose.EmailConfirmation"/> purpose, extracts the user identifier from 
    /// the token payload, and marks the user account as email-confirmed.
    /// If the user is already confirmed, the method returns without changes.
    /// </remarks>
    /// <param name="token">
    /// The email confirmation token recieved from the confirmation link. Cannot be null or empty.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous confirmation operation.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="token"/> is null or empty.
    /// </exception>
    /// <exception cref="InvalidTokenException">
    /// Thrown if the token is invalid, expired, or does not match the expected purpose.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the user referenced by the token payload cannot be found.
    /// </exception>
    public async Task ConfirmEmailAsync(string token, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(token, nameof(token));

        if(!_authTokenService.TryValidateToken(token, AuthTokenPurpose.EmailConfirmation ,out var payload))
        {
            throw new InvalidTokenException();
        }

        var user = await _userRepository.GetUserByIdAsync(payload!.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.IsEmailConfirmed)
        {
            return;
        }

        user.IsEmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user, cancellationToken);
    }

    /// <summary>
    /// Registers a new administrator asynchronously using the specified registration details.
    /// </summary>
    /// <param name="userRegisterDto">
    /// An object containing the administrator's registration information, including username, email, and password. Cannot be
    /// null.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the registration operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the unique identifier of the newly
    /// registered user.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a user with the provided username or email already exists, or if the default user role cannot be
    /// found.
    /// </exception>

    public async Task<int> RegisterAdminAsync(UserRegisterDto userRegisterDto, CancellationToken cancellationToken)
    {
        ValidateRegistrationInput(userRegisterDto);

        var normalizedAdminName = NormalizeInput(userRegisterDto.Username);
        var normalizedEmail = NormalizeInput(userRegisterDto.Email);
        await EnsureUserDoesNotExists(normalizedAdminName, normalizedEmail, cancellationToken);

        var user = CreateUserEntity(userRegisterDto, normalizedAdminName, normalizedEmail);

        await AssignRoleAsync(user, "admin", cancellationToken);

        var userCreated = await _userRepository.AddUserAsync(user, cancellationToken);

        await SendEmailConfirmationLinkAsync(userCreated, cancellationToken);

        return userCreated.Id;
    }

    /// <summary>
    /// Resends the email confirmation link asynchronously using the specified email. 
    /// If more than <c>3 minutes</c> have passed since the last time of the current operation.
    /// </summary>
    /// <param name="toEmail">
    /// The requested email to confirm.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the resend confirmation link operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown if a user with the specified email not exists.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the requested email is null or consists of whitespaces.
    /// </exception>
    public async Task ResendEmailConfirmationLink(string toEmail, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail, nameof(toEmail));

        ValidateEmailPatern(toEmail);

        var normalizedEmail = NormalizeInput(toEmail);
        var user = await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken)
            ?? throw new NotFoundException();

        if (user.IsEmailConfirmed)
        {
            return;
        }

        if(user.EmailConfirmationSentAt.HasValue && user.EmailConfirmationSentAt > DateTime.UtcNow.AddMinutes(-3))
        {
            return;
        }

        await SendEmailConfirmationLinkAsync(user, cancellationToken);

    }
    
    /// <summary>
    /// Authenticates a user asynchronously using the specified login details and generates access and refresh tokens upon successful authentication.
    /// </summary>
    /// <param name="userLoginDto">
    /// The user's login details, including email and password. Cannot be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the login operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous login operation. The task result contains a <see cref="LoginResultDto"/> with the generated access and refresh tokens.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the user's email is not confirmed or the login credentials are invalid.
    /// </exception>
    public async Task<LoginResultDto> LoginAsync(
        UserLoginDto userLoginDto, 
        string? ip, 
        string? userAgent, 
        CancellationToken cancellationToken)
    {
        ValidateLoginInput(userLoginDto);

        var user = await GetActiveUserByEmailAsync(userLoginDto.Email, cancellationToken);

        VerifyPasswordOrThrow(user, userLoginDto.Password);

        var roles = await GetRolesOrThrow(user.Id, cancellationToken);

        var accessToken = CreateAccessToken(user, roles);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, ip, userAgent, cancellationToken);

        return new LoginResultDto(accessToken, refreshToken);
    }

    /// <summary>
    /// Logs out a user by invalidating the provided refresh token. This method marks the refresh token as revoked in 
    /// the database, preventing its future use for obtaining new access tokens.
    /// </summary>
    /// <param name="refreshToken">
    /// The refresh token to be invalidated. Cannot be null or empty.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the logout operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = _refreshTokenHasher.Hash(refreshToken);

        var token = await _userRepository.GetRefreshTokenByHashAsync(hash, cancellationToken);

        if(token is null)
        {
            return;
        }

        token.RevokedAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Exchanges a valid refresh token for a new access token and refresh token. Performs refresh token rotation by revoking
    /// the current token and issuing a new pair.
    /// </summary>
    /// <param name="refreshToken">
    /// The refresh token provided by the client.
    /// </param>
    /// <param name="ip">
    /// The IP address of the client.
    /// </param>
    /// <param name="userAgent">
    /// The user agent string of the client device.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the refresh token operation.
    /// </param>
    /// <returns>
    /// A <see cref="LoginResultDto"/> containing a new access and refresh tokens.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown if the user is not found in data base.
    /// </exception>
    public async Task<LoginResultDto> RefreshTokenAsync(
        string refreshToken, 
        string? ip, 
        string? userAgent, 
        CancellationToken cancellationToken)
    {
        var oldHash = HashRefreshToken(refreshToken);

        var existingToken = await _userRepository.GetRefreshTokenByHashAsync(oldHash, cancellationToken) ?? throw new UnauthorizedAccessException("Invalid refresh token.");
        
        await ValidateExistingRefreshToken(existingToken, cancellationToken);

        var user = await _userRepository.GetUserByIdAsync(existingToken.UserId, cancellationToken) 
        ?? throw new UnauthorizedAccessException("User not found.");

        var roles = await GetRolesOrThrow(user.Id, cancellationToken);

        var tokenPair = await CreateTokenPairAsync(user, roles, ip, userAgent, cancellationToken);

        RevokeToken(existingToken, tokenPair.RefreshTokenHash);

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new LoginResultDto(tokenPair.AccessToken, tokenPair.RefreshToken);
    }
    
    /// <summary>
    /// Revokes all active refresh tokens for the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user whose refresh tokens will be revoked.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the refresh token operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the specified identifier is negative or zero.
    /// </exception>
    public async Task LogoutAllAsync(int userId, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);
        await _userRepository.RevokeAllRefreshTokensForUserAsync(userId, DateTime.UtcNow, cancellationToken);
    }
    
    /// <summary>
    /// Initiates account deletion for the specified user by validating the provided password and sending an account deletion 
    /// confirmation link to the user's confirmed email address.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the currently authenticated user requesting account deletion.
    /// </param>
    /// <param name="password">
    /// The user's current password used to verify the deletion request.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancell the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="password"/> is null, empty, or consists only of white spaces.
    /// </exception>
    /// <exception cref="UnauthorizedAccessExcepiton">
    /// Thrown when the user does not exists, is inactive, is deleted, is not email-confirmed, or when is provided password is invalid.
    /// </exception>
    public async Task RequestAccountDeletionAsync(int userId, string password, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);

        if(user == null || !user.IsActive || user.IsDeleted || !user.IsEmailConfirmed)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        VerifyPasswordOrThrow(user, password);

        await SendAccountDeletionConfirmaitonLinkAsync(user, cancellationToken);
    }

    /// <summary>
    /// Confirms account deletion using the provided account deletion token, revokes all refresh tokens for the user,
    /// and marks the account as deleted.
    /// </summary>
    /// <param name="token">
    /// The account deletion confirmation token received from the email link.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancell the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="InvalidTokenException">
    /// Thrown when the provided token is invalid, expired, or does not match the expected purpose.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="token"/> is null, empty or consists only of white space.
    /// </exception>
    public async Task ConfirmAccountDeletionAsync(string token, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));

        if(!_authTokenService.TryValidateToken(token, AuthTokenPurpose.AccountDeletion ,out var payload))
        {
            throw new InvalidTokenException();
        }

        var user = await _userRepository.GetUserByIdAsync(payload!.UserId, cancellationToken);

        if (user is null || user.IsDeleted && !user.IsActive)
        {
            return;
        }

        user.IsDeleted = true;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.RevokeAllRefreshTokensForUserAsync(user.Id, user.UpdatedAt ,cancellationToken);

        await _userRepository.UpdateUserAsync(user, cancellationToken);
    }
    #region of private methods
    /// <summary>
    /// Generates an email confirmation link for the specified user ID.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user for whom to create the email confirmation link.
    /// </param>
    /// <returns>
    /// The generated <see cref="Uri"/> representing the email confirmation link.
    /// </returns>
    private Uri CreateEmailConfirmationLink(int userId)
    {
        var token = _authTokenService.CreateToken(AuthTokenPurpose.EmailConfirmation, userId, DateTime.UtcNow.AddHours(24));
        return _authLinkGenerator.CreateEmailConfirmationLink(token);
    }

    /// <summary>
    /// Assigns the specified role to the user.
    /// </summary>
    /// <param name="user">
    /// The user entity to which the specified role will be assigned.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token that can be used to cancel the operation.
    /// </param>
    /// <param name="roleName">
    /// The specified role name to assign.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the specified role cannot be found in the repository.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="user"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="roleName"/> is null or consists of white spaces
    /// </exception>
    private async Task AssignRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName, nameof(roleName));

        var role = await _userRepository.GetRoleByNameAsync(roleName, cancellationToken)
            ?? throw new InvalidOperationException($"Role '{roleName}' not found.");
        
        if(user.UserRoles.Any(ur => ur.RoleId == role.Id))
        {
            return;
        }

        user.UserRoles.Add(new UserRole()
        {
            RoleId = role.Id,
        });
    }

    /// <summary>
    /// Creates a new User entity based on the provided registration data.
    /// </summary>
    /// <param name="dto">
    /// The user registration data transfer object containing the user's details.
    /// </param>
    /// <param name="normalizedUsername">
    /// The normalized username for the user.
    /// </param>
    /// <param name="normalizedEmail">
    /// The normalized email address for the user.
    /// </param>
    /// <returns>
    /// The newly created User entity, with hashed password and initialized properties.
    /// </returns>
    private User CreateUserEntity(UserRegisterDto dto, string normalizedUsername, string normalizedEmail)
    {
        User user = new()
        {
            UserName = dto.Username,
            NormalizedUsername = normalizedUsername,
            Email = normalizedEmail,
            NormalizedEmail = NormalizeInput(dto.Email),
            IsActive = true,
            IsDeleted = false,
            IsEmailConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Tokens = []
        };
        user.PasswordHash = PasswordHashing(dto.Password, user);

        return user;
    }

    /// <summary>
    /// Validates the user registration input for required fields, email format, and password rules.
    /// </summary>
    /// <param name="dto">
    /// The registration DTO to validate
    /// </param>
    private static void ValidateRegistrationInput(UserRegisterDto dto)
    {
        ValidateUserRegisterDto(dto);
        ValidateEmailPatern(dto.Email);
        ValidatePasswordPatern(dto.Password);
    }

    /// <summary>
    /// Validates the user login input for required fields and email format.
    /// </summary>
    /// <param name="dto">
    /// The login DTO to validate
    /// </param>
    private static void ValidateLoginInput(UserLoginDto dto)
    {
        ValidateUserLoginDto(dto);
        ValidateEmailPatern(dto.Email);
    }
    
    /// <summary>
    /// Retrieves an active user by their email address. This method checks if a user with the specified email exists and is active (not deleted and marked as active and email confirmed).
    /// If the user is not found or is inactive or not email confirmed, an <see cref="UnauthorizedAccessException"/> is thrown to indicate invalid login credentials.
    /// </summary>
    /// <param name="email">
    /// The email address of the user to retrieve. This value is normalized to ensure a case-insensitive search. Cannot be null or empty.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the active user with the specified email, or an exception is thrown if the user is not found or inactive.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown if no active user with the specified email is found, indicating invalid login credentials.
    /// </exception>
    private async Task<User> GetActiveUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeInput(email);
        var user = await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !user.IsActive || user.IsDeleted || !user.IsEmailConfirmed)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        return user;
    }

    /// <summary>
    /// Verifies the provided password against the stored password hash for the given user. If the verification fails, an <see cref="UnauthorizedAccessException"/> is thrown to indicate invalid login credentials.
    /// </summary>
    /// <param name="user">
    /// The user whose password is to be verified. Must not be null.
    /// </param>
    /// <param name="password">
    /// The password to verify against the user's stored password hash. Must not be null or empty.
    /// </param>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown if the password verification fails, indicating invalid login credentials.
    /// </exception>
    private void VerifyPasswordOrThrow(User user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
    }

    /// <summary>
    /// Validates the specified user login data transfer object to ensure all required fields are present and valid.
    /// </summary>
    /// <param name="dto">
    /// The user login DTO to validate. Must not be null and must contain non-empty values for Email and Password.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="dto"/> is null, or if the Email or Password properties of <paramref name="dto"/> are null or empty.
    /// </exception>
    private static void ValidateUserLoginDto(UserLoginDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto, nameof(dto));

        if (string.IsNullOrEmpty(dto.Email) ||
           string.IsNullOrEmpty(dto.Password))
        {
            throw new ArgumentException("Email and Password are required.");
        }
    }

    /// <summary>
    /// Validates that the user has at least one role and that all roles are properly loaded. If the user has no roles or if any role is not loaded, an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <param name="userId">
    /// The ID of the user whose roles are to be validated.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a read-only collection of the user's roles if validation is successful; otherwise, an exception is thrown.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the user has no roles or if any of the user's roles are not properly loaded, indicating an invalid user state for authentication.
    /// </exception>
    private async Task<IReadOnlyCollection<Role>> GetRolesOrThrow(int userId, CancellationToken cancellationToken)
    {
        var userRoles = await _userRepository.GetUserRolesAsync(userId, cancellationToken);
        var roles = userRoles.Select(ur => ur.Role).ToList();

        if(roles.Count == 0)
        {
            throw new InvalidOperationException("User must have at least one role.");
        }

        if(roles.Any(r => r is null))
        {
            throw new InvalidOperationException("User roles are not loaded properly.");
        }

        return roles;
    }

    /// <summary>
    /// Creates an access token for the specified user and their associated roles. This method generates a JWT access token that includes the user's identity and role claims, allowing the user to authenticate and authorize access to protected resources based on their assigned roles.
    /// </summary>
    /// <param name="user">
    /// The user for whom the access token is being created. Must not be null.
    /// </param>
    /// <param name="roles">
    /// The roles associated with the user. Must not be null or empty.
    /// </param>
    /// <returns>
    /// A JWT access token string that includes the user's identity and role claims.
    /// </returns>
    private string CreateAccessToken(User user, IReadOnlyCollection<Role> roles) =>
        _jwtTokenService.CreateAccessToken(user, roles);

    /// <summary>
    /// Creates a refresh token for the specified user and stores it in the database. This method generates a secure refresh token, hashes it for storage, and associates it with the user's account. The refresh token can be used to obtain new access tokens without requiring the user to re-authenticate, providing a seamless authentication experience while maintaining security.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user for whom the refresh token is being created. Must be a positive integer corresponding to an existing user in the database.
    /// </param>
    /// <param name="ip">
    /// The IP address of the client requesting the refresh token. This value is stored for security auditing and tracking purposes.
    /// </param>
    /// <param name="userAgent">
    /// The user agent string of the client requesting the refresh token. This value is stored for security auditing and tracking purposes, and is truncated to a maximum length of 512 characters if necessary.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the generated refresh token string.
    /// </returns>
    private async Task<string> CreateRefreshTokenAsync(
        int userId, 
        string? ip,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var refreshToken = _refreshTokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = _refreshTokenHasher.Hash(refreshToken);

        var now = DateTime.UtcNow;
        RefreshToken refreshTokenEntity = new()
        {
            UserId = userId,
            TokenHash = refreshTokenHash,
            ExpiresAt = now.AddDays(7),
            CreatedAt = now,
            CreatedByIp = ip,
            UserAgent = userAgent?.Length > 512
                ? userAgent[..512]
                : userAgent
        };

        await _userRepository.SaveRefreshTokenAsync(refreshTokenEntity, cancellationToken);
        return refreshToken;
    }

    /// <summary>
    /// Validates the specified user registration data transfer object to ensure all required fields are present.
    /// </summary>
    /// <param name="dto">The user registration data to validate. Must not be null and must contain non-empty values for Username, Email,
    /// and Password.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="dto"/> is null, or if the Username, Email, or Password properties of <paramref
    /// name="dto"/> are null or empty.</exception>
    private static void ValidateUserRegisterDto(UserRegisterDto dto)
    {
        if (dto is null)
        {
            throw new ArgumentException("Invalid user registration data.");
        }

        if (string.IsNullOrEmpty(dto.Username) ||
           string.IsNullOrEmpty(dto.Email) ||
           string.IsNullOrEmpty(dto.Password))
        {
            throw new ArgumentException("Username, Email, and Password are required.");
        }
    }

    /// <summary>
    /// Validates that the specified email address conforms to a standard email format.
    /// </summary>
    /// <remarks>This method checks for a basic email pattern, including the presence of an '@' symbol and a
    /// domain. It does not guarantee that the email address exists or is deliverable.</remarks>
    /// <param name="email">The email address to validate. Cannot be null or empty.</param>
    /// <exception cref="ArgumentException">Thrown if the email address does not match the expected format.</exception>
    private static void ValidateEmailPatern(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase))
        {
            throw new ArgumentException("Invalid email format.");
        }
    }

    /// <summary>
    /// Validates that the specified password meets the required pattern of containing at least eight characters,
    /// including both letters and numbers.
    /// </summary>
    /// <param name="password">The password string to validate. Must be at least eight characters long and contain both letters and numbers.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="password"/> does not meet the required pattern of at least eight characters with both
    /// letters and numbers.</exception>
    private static void ValidatePasswordPatern(string password)
    {
        string passwordPattern = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$";
        if (!Regex.IsMatch(password, passwordPattern))
        {
            throw new ArgumentException("Password must be at least 8 characters long and contain both letters and numbers.");
        }
    }

    /// <summary>
    /// Converts the specified input string to its uppercase equivalent using the invariant culture.
    /// </summary>
    /// <param name="input">The string to normalize. If null, a NullReferenceException will be thrown.</param>
    /// <returns>A new string containing the uppercase representation of the input, using invariant culture rules.</returns>
    private static string NormalizeInput(string input) => input.Trim().ToUpperInvariant();

    /// <summary>
    /// Generates a hashed representation of the specified password for the given user.
    /// </summary>
    /// <param name="password">The plain text password to hash. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <param name="user">The user for whom the password is being hashed. Used as context for the hashing operation.</param>
    /// <returns>A hashed string representing the password, suitable for storage and later verification.</returns>
    /// <exception cref="ArgumentException">Thrown if the password is null, empty, or consists only of white-space characters.</exception>
    private string PasswordHashing(string password, User user)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }
        return _passwordHasher.HashPassword(user, password);
    }

    /// <summary>
    /// Ensures that no user exists with the specified normalized username or normalized email.
    /// </summary>
    /// <remarks>
    /// This method checks the user repository for existing users with the given normalized username and email. 
    /// If a user is found with either identifier, an <see cref="InvalidOperationException"/> is thrown to indicate that the user 
    /// cannot be created due to existing records.
    /// </remarks>
    /// <param name="normalizedUsername">
    /// The normalized username to check for existing users.
    /// </param>
    /// <param name="normalizedEmail">
    /// The normalized email to check for existing users.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// The task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a user with the provided username or email already exists.
    /// </exception>
    private async Task EnsureUserDoesNotExists(string normalizedUsername, string normalizedEmail, CancellationToken cancellationToken)
    {
        if (await _userRepository.GetUserByNormalizedUsernameAsync(normalizedUsername, cancellationToken) is not null)
        {
            throw new InvalidOperationException("A user with the provided username already exists.");
        }
        
        if (await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            throw new InvalidOperationException("A user with the provided email already exists.");
        }
    }

    /// <summary>
    /// Resends the confirmation link asynchronously to the requested user. Updates the user entity, when last time the confirmation link was last sent and 
    /// when last time the entity was last updated.
    /// </summary>
    /// <param name="user">
    /// The user that requested the resend confirmation link operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// The task representing the asynchronous operation.
    /// </returns>
    private async Task SendEmailConfirmationLinkAsync(User user, CancellationToken cancellationToken)
    {
        var confirmationLink = CreateEmailConfirmationLink(user.Id);

        await _emailSender.SendEmailConfirmationAsync(user.Email, confirmationLink, cancellationToken);

        //Save when the confirmation link was sended
        user.EmailConfirmationSentAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user, cancellationToken);
    }
    
    /// <summary>
    /// Computes a secure hash of the provided refresh token. The hash is issued for storage and lookup instead of the raw token  value.
    /// </summary>
    /// <param name="refreshToken">
    /// The refresh token to hash.
    /// </param>
    /// <returns>
    /// The hashed representation of the refresh token.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="refreshToken"/> is null or consists of white spaces.
    /// </exception>
    private string HashRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken, nameof(refreshToken));

        return _refreshTokenHasher.Hash(refreshToken);
    }

    /// <summary>
    /// Validates the state of an existing refresh token. Ensures the token has not expired or been revoked.
    /// </summary>
    /// <param name="token">
    /// The refresh token to validate.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation of revoking all refresh tokens of the user if the provided
    /// <paramref name="token"/> is not null. This operation should be run for security purposes in case of the data leakage.
    /// </param>
    /// <returns>
    /// A task representing an asynchronous operation.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown if the token is expired or revoked.
    /// </exception>
    private async Task ValidateExistingRefreshToken(RefreshToken token, CancellationToken cancellationToken)
    {
        if (token.RevokedAt is not null)
        {
            await _userRepository.RevokeAllRefreshTokensForUserAsync(token.UserId, DateTime.UtcNow, cancellationToken);
            throw new UnauthorizedAccessException("Refresh token has been revoked.");
        }

        if(token.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token has expired.");
        }
    }

    /// <summary>
    /// Revokes a refresh token and records the hash of the token that replaced it.
    /// </summary>
    /// <param name="token">
    /// The refresh token to revoke.
    /// </param>
    /// <param name="replacedByTokenHash">
    /// Hash of the new refresh token that rplaced the revoked token.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided <paramref name="token"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the provided <paramref name="replacedByTokenHash"/> is null or consists of the white spaces.
    /// </exception> 
    private static void RevokeToken(RefreshToken token, string replacedByTokenHash)
    {
        ArgumentNullException.ThrowIfNull(token, nameof(token));
        ArgumentException.ThrowIfNullOrWhiteSpace(replacedByTokenHash, nameof(replacedByTokenHash));

        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByTokenHash = replacedByTokenHash;
    }

    /// <summary>
    /// Generates a new access token and refresh token for the specified user. The refresh token is persisted in 
    /// the data store and its hash is returned for rotation tracking.
    /// </summary>
    /// <param name="user">
    /// The authenticated user.
    /// </param>
    /// <param name="roles">
    /// Roles associated with the current <paramref name="user"/>.
    /// </param>
    /// <param name="ip">
    /// The client IP address.
    /// </param>
    /// <param name="userAgent">
    /// The client user agent.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation of creating a refresh token.
    /// </param>
    /// <returns>
    /// A tuple containing the access token, refresh token. and refresh token hash.
    /// </returns>
    private async Task<(string AccessToken, string RefreshToken, string RefreshTokenHash)> CreateTokenPairAsync(
        User user, 
        IReadOnlyCollection<Role> roles, 
        string?  ip, 
        string? userAgent, 
        CancellationToken cancellationToken)
    {
        var accessToken = CreateAccessToken(user, roles);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, ip, userAgent, cancellationToken);
        var refreshTokenHash = _refreshTokenHasher.Hash(refreshToken);

        return (accessToken, refreshToken, refreshTokenHash);
    }

    /// <summary>
    /// Generates and sends an account deletion confirmation link to the specified user's email address and updates the user's
    /// last modification timestamp.
    /// </summary>
    /// <param name="user">
    /// The user whose account deletion confirmation email should be sent.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    private async Task SendAccountDeletionConfirmaitonLinkAsync(User user, CancellationToken cancellationToken)
    {
        var confirmationLink = CreateDeleteAccountConfirmationLink(user.Id);
        await _emailSender.SendAccountDeletionConfirmationAsync(user.Email, confirmationLink, cancellationToken);

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user, cancellationToken);
    }

    /// <summary>
    /// Creates an account deletion confirmation link containing a time limited token for the specified user identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the user for whom the confirmation link is created.
    /// </param>
    /// <returns>
    /// A fully qualified <see cref="Uri"/> that points to the account deletion confirmation endpoint 
    /// </returns>
    private Uri CreateDeleteAccountConfirmationLink(int id)
    {
        var token = _authTokenService.CreateToken(AuthTokenPurpose.AccountDeletion, id, DateTime.UtcNow.AddHours(24));
        return _authLinkGenerator.CreateAccountDeletionConfirmationLink(token);
    }

    #endregion of private methods
}
