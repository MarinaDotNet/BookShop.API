namespace BookShop.API.DTOs.Auth;

/// <summary>
/// Represents the data required to update the account password of the currently authenticated user.
/// </summary>
/// <param name="CurrentPassword">
/// The current password of the account.
/// </param>
/// <param name="NewPassword">
/// The new password to be set.
/// </param>
public sealed record UpdatePasswordDto(string CurrentPassword, string NewPassword);