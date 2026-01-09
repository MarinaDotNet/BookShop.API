using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.API.Models;

/// <summary>
/// Represents a refresh token for a user's session.
/// </summary>
[Table("RefreshTokens")]
public class RefreshToken
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }

    [Column("Token", TypeName = "TEXT")]
    public string Token { get; set; } = string.Empty;

    [Column("ExpiresAt", TypeName = "timestamp without time zone")]
    public DateTime ExpiresAt { get; set; }

    [Column("CreatedAt", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
