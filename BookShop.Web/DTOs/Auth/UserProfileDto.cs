namespace BookShop.Web.DTOs.Auth;

public sealed record UserProfileDto(int Id, string Username, string Email, IReadOnlyCollection<string> Roles, bool IsEmailConfirmed);