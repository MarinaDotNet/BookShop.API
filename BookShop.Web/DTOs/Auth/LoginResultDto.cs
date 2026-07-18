namespace BookShop.Web.DTOs.Auth;

/// <summary>
/// Represents the authentication result returned by the BookShop API.
/// </summary>
/// <param name="AccessToken">
/// JWT access token.
/// </param>
/// <param name="RefreshToken">
/// Refresh token.
/// </param>
public sealed record LoginResultDto(string AccessToken, string RefreshToken);