namespace BookShop.API.Models;

/// <summary>
/// Container for authentication action-token types used in confirmation links (email confiramation, password reset, etc.).
/// Those tokens are not JWT access/refresh tokens.
/// </summary>
public class AuthTokens
{
    /// <summary>
    /// The purpose of the an auth action-token (used in confirmation links).
    /// </summary>
    public enum AuthTokenPurpose
    {
        EmailConfirmation,
        PasswordReset,
        EmailChange,
        AccountDeletion,
        SensitiveChange
    }

    /// <summary>
    /// The payload encoded into an auth action-token.
    /// </summary>
    public record AuthTokenPayload(int UserId, AuthTokenPurpose Purpose, DateTime ExpiresAtUtc, string? NewEmail = null);
}
