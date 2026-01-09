using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.API.Models.Auth;

/// <summary>
/// Represents the many-to-many relationship between <see cref="User"/> and <see cref="Role"/>.
/// </summary>
[Table("UserRoles")]
public class UserRole
{
    [Column("UserId")]
    public int UserId { get; set; }

    [Column("RoleId")]
    public int RoleId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;
}
