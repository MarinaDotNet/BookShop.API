using BookShop.API.DTOs.Auth;
using BookShop.API.Models.Auth;
using BookShop.API.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using System.Threading;

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
public class AuthServices(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

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
        ValidateUserRegisterDto(userRegisterDto);
        ValidateEmailPatern(userRegisterDto.Email);
        ValidatePasswordPatern(userRegisterDto.Password);

        if(await IsUsernameExists(userRegisterDto.Username, cancellationToken))
        {
            throw new InvalidOperationException("A user with the provided username already exists.");
        }

        if(await IsEmailExists(userRegisterDto.Email, cancellationToken))
        {
            throw new InvalidOperationException("A user with the provided email already exists.");
        }

        User user = new()
        {
            UserName = userRegisterDto.Username,
            NormalizedUsername = NormalizeInput(userRegisterDto.Username),
            Email = userRegisterDto.Email,
            NormalizedEmail = NormalizeInput(userRegisterDto.Email),
            IsActive = true,
            IsDeleted = false,
            IsEmailConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Tokens = []
        };

        user.PasswordHash = PasswordHashing(userRegisterDto.Password, user);

        var role = await _userRepository.GetRoleByNameAsync("user", cancellationToken) 
            ?? throw new InvalidOperationException("Default role 'user' not found.");

        user.UserRoles.Add(new UserRole()
        {
                RoleId = role.Id,
        });

        var userCreated = await _userRepository.AddUserAsync(user, cancellationToken);

        return userCreated.Id;
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
    /// Determines whether a user with the specified username exists in the system.
    /// </summary>
    /// <param name="username">The username to check for existence. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if a user with the
    /// specified username exists; otherwise, <see langword="false"/>.</returns>
    private async Task<bool> IsUsernameExists (string username, CancellationToken cancellationToken)
    {
        var userName = await _userRepository.GetUserByNormalizedUsernameAsync(NormalizeInput(username), cancellationToken);
        
        return userName is not null;
    }

    /// <summary>
    /// Determines whether a user account with the specified email address exists.
    /// </summary>
    /// <param name="email">The email address to check for existence. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if a user with the
    /// specified email exists; otherwise, <see langword="false"/>.</returns>
    private async Task<bool> IsEmailExists(string email, CancellationToken cancellationToken)
    {
        var userEmail = await _userRepository.GetUserByNormalizedEmailAsync(NormalizeInput(email), cancellationToken);
        return userEmail is not null;
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
}
