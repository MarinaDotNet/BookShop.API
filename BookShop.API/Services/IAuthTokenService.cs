using static BookShop.API.Models.AuthTokens;

namespace BookShop.API.Services;

/// <summary>
/// Creates and validates authentication tokens for various purposes such as email confirmation, password reset, etc. that used in confirmation links.
/// Backed by cryptographic data protection to ensure token integrity and confidentiality.
/// </summary>
public interface IAuthTokenService
{
    /// <summary>
    /// Creates an authentication token for the specified purpose and user.
    /// </summary>
    /// <param name="purpose">The token purpose (must match the endpoint/action)</param>
    /// <param name="userId">The user identifier the token is issued for.</param>
    /// <param name="expiresAtUtc">UTC expiration timestamp for the token.</param>
    /// <param name="newEmail">Optional new email (used only for email change confirmation)</param>
    /// <returns>The protected token string suitable for including in a URL.</returns>
    string CreateToken(AuthTokenPurpose purpose, int userId, DateTime expiresAtUtc, string? newEmail = null);

    /// <summary>
    /// Attempts to read and validate the provided authentication token and returns its payload if valid..
    /// </summary>
    /// <param name="token">The protected token string.</param>
    /// <param name="expectedPurpose">Expected purpose for the current operation.</param>
    /// <param name="payload">The decoded payload when validation succeeds.</param>
    /// <returns>True if the token is valid: otherwise false.</returns>
    bool TryValidateToken(string token, AuthTokenPurpose expectedPurpose, out AuthTokenPayload? payload);
}
