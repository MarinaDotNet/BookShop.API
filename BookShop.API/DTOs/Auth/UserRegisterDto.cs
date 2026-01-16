namespace BookShop.API.DTOs.Auth;

/// <summary>
/// Represents the data required to register a new user account.
/// </summary>
/// <param name="Username">The username to associate with the new user account. Cannot be null or empty.</param>
/// <param name="Email">The email address of the user. Must be a valid email format and cannot be null or empty.</param>
/// <param name="Password">The password for the new user account. Cannot be null or empty.</param>
public sealed record UserRegisterDto(string Username, string Email, string Password);
