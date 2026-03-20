namespace BookShop.API.DTOs.Auth;

/// <summary>
/// Represents the data required to reset a user's password.
/// </summary>
/// <param name="Token">
/// The password reset token received via email.
/// </param>
/// <param name="NewPassword">
/// A new password to be set for the account.
/// </param>
public sealed record ResetPasswordDto(string Token, string NewPassword);