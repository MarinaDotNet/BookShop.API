namespace BookShop.API.Services;

/// <summary>
/// Represents the data required for a user to log in to their account.
/// </summary>
/// <param name="Email">
/// The email address of the user attempting to log in. Must be a valid email format and cannot be null or empty.
/// </param>
/// <param name="Password">
/// The password of the user attempting to log in. Cannot be null or empty.
/// </param>
public sealed record UserLoginDto(string Email, string Password);