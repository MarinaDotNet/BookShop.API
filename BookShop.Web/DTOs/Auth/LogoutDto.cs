namespace BookShop.Web.DTOs.Auth;

/// <summary>
/// Represents a request to log out the current user by providing the refresh token that should be revoked.
/// </summary>
/// <param name="RefreshToken">
/// The refresh token to be invalidated.
/// </param>
public sealed record LogoutDto(string RefreshToken);