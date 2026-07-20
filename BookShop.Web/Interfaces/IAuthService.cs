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
}