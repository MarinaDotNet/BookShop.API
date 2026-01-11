using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.API.Models.Auth;

/// <summary>
/// Represents a refresh token for a user's session.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime? RevokedAt { get; set; }
    
    public string? ReplacedByTokenHash { get; set; }
    
    public string? CreatedByIp { get; set; }
    
    public string? UserAgent { get; set; }
}
