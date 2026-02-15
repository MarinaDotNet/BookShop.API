namespace BookShop.API.DTOs.Auth;

/// <summary>
/// Represents the result of a successful login attempt, containing the access token and refresh token for the authenticated user.
/// </summary>
/// <param name="AccessToken">
/// The JWT access token issued to the user upon successful authentication. This token is used to authorize subsequent requests to protected resources and typically has a short expiration time.
/// </param>
/// <param name="RefreshToken">
/// The refresh token issued to the user upon successful authentication. This token is used to obtain a new access token when the current one expires and typically has a longer expiration time than the access token.
/// </param>
public sealed record LoginResultDto(string AccessToken, string RefreshToken);