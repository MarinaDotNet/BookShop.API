using System.ComponentModel.DataAnnotations;

namespace BookShop.Web.DTOs.Auth;

/// <summary>
/// Represents the credentials required to authenticate a user.
/// </summary>
public sealed class UserLoginDto
{
    /// <summary>
    /// User email.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User password.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}