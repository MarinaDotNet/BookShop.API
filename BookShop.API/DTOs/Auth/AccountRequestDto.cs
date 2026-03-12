namespace BookShop.API.DTOs.Auth;

/// <summary>
/// Represents the data required to recover an user account
/// </summary>
/// <param name="Email">
/// The email address of the account requested to be recovered
/// </param>
public sealed record AccountRequestDto (string Email);