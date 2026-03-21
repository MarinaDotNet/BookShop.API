using BookShop.API.Models.Auth;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace BookShop.API.DTOs.Auth;

/// <summary>
/// Represents the profile infromation of a sudr returned by authentication-related endpoints.
/// </summary>
/// <param name="Id">
/// The unique identifier of the user.
/// </param>
/// <param name="Email">
/// The email address of the user.
/// </param>
/// <param name="UserName">
/// The user name of the user.
/// </param>
/// <param name="Roles">
/// The names of the roles assigned to the user.
/// </param>
/// <param name="IsEmailConfirmed">
/// Indicates whether the user's email address has been confirmed.
/// </param>
public sealed record UserDto(int Id, string Email, string UserName, IEnumerable<string> Roles, bool IsEmailConfirmed);