namespace BookShop.API.DTOs.Auth;

/// <summary>
/// Represents the data required to initiate a password reset request.
/// </summary>
/// <param name="Email">
/// The email address associated with the account.
/// </param>
public sealed record ForgotPasswordDto(string Email);