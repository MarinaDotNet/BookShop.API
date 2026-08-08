using BookShop.Web.DTOs.Auth;

namespace BookShop.Web.Interfaces;

/// <summary>
/// Provides authentication operations for the BookShop Web application.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user using email and password.
    /// </summary>
    /// <param name="userLoginDto">
    /// The user's login credentials.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// An object containing authentication tokens and related login information.
    /// </returns>
    Task<LoginResultDto> LoginAsync(UserLoginDto userLoginDto, CancellationToken cancellationToken);

    /// <summary>
    /// Logs out the current user by revoking the refresh token, signing out of the local authentication session, and removing the
    /// authentication cookie.
    /// </summary>
    /// <param name="refreshToken">
    /// The refresh token to be invalidated.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchrounous operation.
    /// </returns>
    Task LogOutAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="registerDto">
    /// The registration data containing the user's username, email address, and password.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancell the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="registerDto"/> is <see langword="null"/>. 
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the registration data is invalid.
    /// </exception>
    Task RegisterAsync(UserRegisterDto registerDto, CancellationToken cancellationToken);

    Task<UserProfileDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}