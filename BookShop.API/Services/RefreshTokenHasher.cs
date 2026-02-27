using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

namespace BookShop.API.Services;

/// <summary>
/// Prodivedes a secure hashing implementation for refresh tokens.
/// </summary>
/// <remarks>
/// Uses HMAC-SHA256 with a secret pepper loaded from configuration (Security:RefreshTokenPepper).
/// The resulting hash is stored instead of the raw refresh token to prevent token leakage in case of database compromise.
/// </remarks>
public sealed class RefreshTokenHasher(IConfiguration configuration) : IRefreshTokenHasher
{
    private readonly string _pepper = configuration["Security:RefreshTokenPepper"]
        ?? throw new InvalidOperationException("Security:RefreshTokenPepper not configured");

    /// <summary>
    /// Computes a secure hash of the specified refresh token.
    /// </summary>
    /// <param name="token">
    /// The raw refresh token to hash. Cannot be null, empty, or whitespace.
    /// </param>
    /// <returns>
    /// A hexadecimal-encoded HMAC-SHA256 hash of the token combined with a secret pepper.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="token" /> is null, empty, or whitespace.
    /// </exception>
    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));

        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(_pepper), tokenBytes);

        return Convert.ToHexString(hash);
    }
}
