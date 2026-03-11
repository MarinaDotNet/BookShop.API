namespace BookShop.API.DTOs.Auth;

/// <summary>
/// Represents the data required to delete a user account
/// </summary>
/// <param name="Password">
/// The password of the account requested to be deleted
/// </param>
public sealed record AccountDeleteDto(string Password);