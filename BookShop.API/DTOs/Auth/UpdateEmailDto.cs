namespace BookShop.API.DTOs.Auth;

/// <summary>
/// Represents the data required to update the account email of the currently authenticated user.
/// </summary>
/// <param name="NewEmail">
/// The new email address to be set.
/// </param>
/// <param name="CurrentPassword">
/// The current password of the account.
/// </param>
public sealed record UpdateEmailDto(string NewEmail, string CurrentPassword);