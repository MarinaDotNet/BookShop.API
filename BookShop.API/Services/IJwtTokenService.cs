using BookShop.API.Models.Auth;

namespace BookShop.API.Services;

/// <summary>
/// Generates JWT access tokens for authenticated users
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Creates a single JWT access token containing user identity and role claims.
    /// </summary>
    /// <param name="user">
    /// The authenticated user.
    /// </param>
    /// <param name="roles">
    /// Roles assigned to the user.
    /// </param>
    /// <returns>
    /// A serialized JWT access token.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="user"/> or <paramref name="roles"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if JWT configuration is missing or invalid.
    /// </exception>
    string CreateAccessToken(User user, IReadOnlyCollection<Role> roles);
}
