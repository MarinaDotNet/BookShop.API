namespace BookShop.API.Services;

/// <summary>
/// Defines a contract for generating authentication-related confirmation links.
/// </summary>
/// <remarks>
/// Implenentations of this interface are responsible for creating absolute URIs for various authentication actions such as:
/// - email confirmation 
/// - password reset 
/// - email change confirmation
/// - account deletion confirmation
/// - sensitive information change confirmation
/// 
/// The generated links typically embedded in emails and must be safe to expose publicly. Token generation and validation is handled separately 
/// and are not the responsibility of this interface.
/// </remarks>
public interface IAuthLinkGenerator
{
    /// <summary>
    /// Creates a confirmation link for email verification.
    /// </summary>
    Uri CreateEmailConfirmationLink(string token);

    /// <summary>
    /// Creates a password reset link.
    /// </summary>
    Uri CreatePasswordResetLink(string token);

    /// <summary>
    /// Creates a confirmation link for email change verification.
    /// </summary>
    Uri CreateEmailChangeConfirmationLink(string token);

    /// <summary>
    /// Creates a confirmation link for account deletion verification.
    /// </summary>
    Uri CreateAccountDeletionConfirmationLink(string token);

    /// <summary>
    /// Creates a confirmation link for sensitive information change verification.
    /// </summary>
    Uri CreateSensitiveChangeConfirmationLink(string token);
}
