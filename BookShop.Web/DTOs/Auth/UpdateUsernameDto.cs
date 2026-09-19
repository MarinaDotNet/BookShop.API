namespace BookShop.Web.Dtos.Auth;

/// <summary>
/// Represents the data required to update a user's username.
/// </summary>
public sealed record UpdateUsernameDto(string NewUserName);