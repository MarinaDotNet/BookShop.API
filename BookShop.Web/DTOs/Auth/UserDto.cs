namespace BookShop.Web.DTOs.Auth;

public sealed record UserDto(int Id, string Email, string UserName, IEnumerable<string> Roles, bool IsEmailConfirmed);