namespace BookShop.API.Services;

/// <summary>
/// Provides hashing for sensitive tokens before persistance.
/// </summary>
public interface IRefreshTokenHasher
{
    /// <summary>
    /// Hashes the specified token using a secret pepper.
    /// </summary>
    /// <param name="token">
    /// The raw token value.
    /// </param>
    /// <returns>
    /// A hexadecimal hash string.
    /// </returns>
    string Hash(string token);
}
