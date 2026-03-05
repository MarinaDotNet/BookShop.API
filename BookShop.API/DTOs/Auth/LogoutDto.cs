namespace BookShop.API.DTOs.Auth;

/// <summary>
/// DTO for logout request, containing the refresh token to be invalidated.
/// </summary>
/// <param name="RefreshToken">
/// The refresh token to be invalidated.
/// </param>
public sealed record LogoutDto(string RefreshToken);