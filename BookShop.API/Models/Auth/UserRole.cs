using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.API.Models.Auth;

/// <summary>
/// Represents the many-to-many relationship between <see cref="User"/> and <see cref="Role"/>.
/// </summary>
public class UserRole
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
