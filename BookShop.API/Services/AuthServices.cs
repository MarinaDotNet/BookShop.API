using BookShop.API.DTOs.Auth;
using BookShop.API.Exceptions;
using BookShop.API.Models.Auth;
using BookShop.API.Repositories;
using Microsoft.AspNetCore.Identity;
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
    IAuthEmailSender emailSender)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly IAuthTokenService _authTokenService = authTokenService;
    private readonly IAuthLinkGenerator _authLinkGenerator = authLinkGenerator;
    private readonly IAuthEmailSender _emailSender = emailSender;

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

        //Create Confirmation Link
        var confirmationLink = CreateEmailConfirmationLink(userCreated.Id);

        //Send Confirmation Email
        await _emailSender.SendEmailConfirmationAsync(userCreated.Email, confirmationLink, cancellationToken);

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

        var confirmationLink = CreateEmailConfirmationLink(userCreated.Id);

        await _emailSender.SendEmailConfirmationAsync(userCreated.Email, confirmationLink, cancellationToken);

        return userCreated.Id;
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

    #endregion of private methods
}
