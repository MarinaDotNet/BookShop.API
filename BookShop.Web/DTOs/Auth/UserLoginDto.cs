namespace BookShop.Web.DTOs.Auth;

/// <summary>
/// Represents the credentials required to authenticate a user.
/// </summary>
/// <param name="Email">
/// The user's email.
/// </param>
/// <param name="Password">
/// The account password.
/// </param>
public sealed record UserLoginDto(string Email, string Password);