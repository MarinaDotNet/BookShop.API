using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace BookShop.API.Services;

/// <summary>
/// Generates crypthography secure refresh tokens.
/// </summary>
/// <remarks>
/// Uses a secure random number generator to produce high-entropy tokens suitable for long-lived authentication sessions.
/// The generated token is URL-safe and intended to be stored only in hashed form.
/// </remarks>
public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int RefreshTokenSize = 64;

    /// <summary>
    /// Generates a new secure refresh token.
    /// </summary>
    /// <returns>
    /// A URL-safe, crypthographically secure refresh token string.
    /// </returns>
    /// <remarks>
    /// The returned token should be hashed vefore veing persisted.
    /// Plain tokens must never be stored in the database.
    /// </remarks>
    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(RefreshTokenSize);

        return WebEncoders.Base64UrlEncode(bytes);
    }
}
