namespace BookShop.API.Services;

/// <summary>
/// Generates crypthography secure refresh tokens.
/// </summary>
public interface IRefreshTokenGenerator
{
    /// <summary>
    /// Generates a new refresh token.
    /// </summary>
    /// <returns>
    /// A URL-safe refresh token string.
    /// </returns>
    string GenerateRefreshToken();
}
