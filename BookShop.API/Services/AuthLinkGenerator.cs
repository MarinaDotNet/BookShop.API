using BookShop.API.Infrastructure;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace BookShop.API.Services;

/// <summary>
/// Generates absolute authentication-related links based on application configuration.
/// </summary>
/// <remarks>
/// This class is responsible for building public-facing URLs used in authentication and account security workflows. 
/// It combines a configured public base URL with predefined paths and securely encoded tokens.
/// This generator doesnot perform token generation or validation - it assumes that tokens are already generated and validated by a dedicated token service.
/// This class is environment-agnostic and relies entirely on configuration for determining the public host.
/// </remarks>
public class AuthLinkGenerator : IAuthLinkGenerator
{
    private readonly Uri _baseUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthLinkGenerator"/> class.
    /// </summary>
    /// <param name="options">Options class containing the base URL of the application.</param>
    /// <exception cref="InvalidOperationException">Thrown if the public base URL is not configured.</exception>
    public AuthLinkGenerator(IOptions<AppUrlOptions> options)
    {
        var _baseUrlString = options.Value.PublicBaseUrl
            ?? throw new InvalidOperationException("PublicBaseUrl not configured.");

        _baseUri = new Uri($"https://{_baseUrlString}/");
    }

    /// <summary>
    /// Creates a confirmation link for account deletion verification.
    /// </summary>
    public Uri CreateAccountDeletionConfirmationLink(string token) =>
        Build("auth/confirm-account-deletion", token);

    /// <summary>
    /// Creates a confirmation link for email change verification.
    /// </summary>
    public Uri CreateEmailChangeConfirmationLink(string token) =>
        Build("auth/confirm-email-change", token);

    /// <summary>
    /// Creates a confirmation link for email verification.
    /// </summary>
    public Uri CreateEmailConfirmationLink(string token) => 
        Build("auth/confirm-email", token);

    /// <summary>
    /// Creates a password reset link.
    /// </summary>
    public Uri CreatePasswordResetLink(string token) =>
        Build("auth/reset-password", token);

    /// <summary>
    /// Creates a confirmation link for sensitive information change verification.
    /// </summary>
    public Uri CreateSensitiveChangeConfirmationLink(string token) =>
        Build("auth/confirm-sensitive-change", token);

    /// <summary>
    /// Builds an absolute authentication link by combining a route path with a URL-safe token.
    /// </summary>
    /// <remarks>
    /// This method constructs a fully-qualified HTTPs URI using the configured public base URL, a relative authentication route, 
    /// and securely encoded token passed as a query parameter.
    /// </remarks>
    /// <param name="path">
    /// The relative route path used for the authentication action (e.g. <c>auth/confirm-emalil</c>).
    /// Must not be <c>null</c>, <c>empty</c>, or <c>whitespace</c>.
    /// </param>
    /// <param name="token">
    /// The authentication token to be embedded in the generated link.
    /// Must not be <c>null</c>, <c>empty</c>, or <c>whitespace</c>.
    /// </param>
    /// <returns>A fully-qualified <see cref="Uri" /> representing the authentication confirmation link.</returns>
    /// <exception cref="ArgumentException"> Thrown if <paramref name="path"/> or <paramref name="token"/> is null, empty, or whitespace.</exception>
    private Uri Build(string path, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var encodedToken = Uri.EscapeDataString(token);
        var relativeUri = $"{path}?token={encodedToken}";

        return new Uri(_baseUri, relativeUri);
    }
}
