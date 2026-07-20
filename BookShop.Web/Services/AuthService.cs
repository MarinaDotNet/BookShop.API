using System.Text.RegularExpressions;
using BookShop.Web.Constants;
using BookShop.Web.Interfaces;
using BookShop.Web.DTOs.Auth;

namespace BookShop.Web.Services;

/// <summary>
/// Provides authentication functionallity by communicating with the BookShop API.
/// </summary>
/// <remarks>
/// This service validates user input before sending authentication requests to the backend API.
/// </remarks>
public class AuthServcie(IApiClient apiClient) : IAuthService
{
    private readonly IApiClient _apiClient = apiClient
        ?? throw new ArgumentNullException(nameof(apiClient));

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
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="userLoginDto"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <see cref="UserLoginDto.Password"/> is <c>null</c>, empty, consists only of white-space characters or length is 
    /// less than 8 characters long.
    /// </exception>
    public async Task<LoginResultDto> LoginAsync(UserLoginDto userLoginDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userLoginDto, nameof(userLoginDto));

        ValidateEmailPatern(userLoginDto.Email);

        if(string.IsNullOrWhiteSpace(userLoginDto.Password) || userLoginDto.Password.Length < 8)
        {
            throw new ArgumentException("Password must contain at least 8 characters.", nameof(userLoginDto));
        }

        return await _apiClient.PostAsync<UserLoginDto, LoginResultDto>(ApiRoutes.V1.Auth.LogIn, userLoginDto, cancellationToken);
    }

    /// <summary>
    /// Logs out the current user by sending the refresh token to the API, revoking the authentication session, and removing the
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
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="refreshToken"/> is <see langword="null"/>. 
    /// </exception>
    public async Task LogOutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken, nameof(refreshToken));

        await _apiClient.PostAsync(ApiRoutes.V1.Auth.LogOut, new LogoutDto(refreshToken), cancellationToken);
    }


    /// <summary>
    /// Validates that the specified email address conforms to a standard email format.
    /// </summary>
    /// <remarks>
    /// This method checks for a basic email pattern, including the presence of an '@' symbol and a
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
}