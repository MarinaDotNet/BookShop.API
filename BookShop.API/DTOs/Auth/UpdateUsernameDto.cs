namespace BookShop.API.DTOs.Auth;

/// <summary>
/// Represents the data required to update the account username of the current user.
/// </summary>
/// <param name="NewUserName">
/// The new user name.
/// </param>
public sealed record UpdateUsernameDto (string NewUserName);