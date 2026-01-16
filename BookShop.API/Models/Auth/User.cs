using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.API.Models.Auth;

/// <summary>
/// Represents an application user.
/// </summary>
public class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string NormalizedUsername { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public bool IsEmailConfirmed { get; set; } = false;

    public DateTime  CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; } 

    public ICollection<RefreshToken> Tokens { get; set; } = new HashSet<RefreshToken>();

    public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
}

