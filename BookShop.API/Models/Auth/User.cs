using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.API.Models.Auth;

/// <summary>
/// Represents an application user.
/// </summary>
[Table("Users")]
public class User
{
    [Key]
    [Column("Id", TypeName ="SERIAL")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Username", TypeName = "VARCHAR(50)")]
    public string Username { get; set; } = string.Empty;

    [Column("Email", TypeName = "VARCHAR(100)")]
    public string Email { get; set; } = string.Empty;

    [Column("PasswordHash", TypeName = "TEXT")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("IsActive", TypeName = "BOOLEAN")]
    public bool IsActive { get; set; } = true;

    [Column("IsDeleted", TypeName = "BOOLEAN")]
    public bool IsDeleted { get; set; } = false;

    [Column("CreatedAt", TypeName = "timestamp without time zone")]
    public DateTime  CreatedAt { get; set; }

    [Column("UpdatedAt", TypeName = "timestamp without time zone")]
    public DateTime UpdatedAt { get; set; } 

    public ICollection<RefreshToken> Tokens { get; set; } = new HashSet<RefreshToken>();

    public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
}

