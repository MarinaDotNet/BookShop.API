namespace BookShop.API.Infrastructure;

/// <summary>
/// Represents application level URL configuration used for generation public-facing links.
/// </summary>
/// <remarks>
/// This options class holds the base public URL of the application, that is used to generate links for various authentication-related actions such as email confirmation, password reset, etc.
/// 
/// This class is intend to be bound to the <c>App</c> configuration section and consumed via <see cref="Microsoft.Extensions.Options.IOptions{TaskContinuationOptions}"/>
/// </remarks>
public sealed class AppUrlOptions
{

    /// <summary>
    /// Gets the public base URL of the application used to generate absolute links.
    /// </summary>
    /// <remarks>
    /// This value must not include a trailing slash. For example: <c>https://example.com</c>
    /// Example values could be:
    /// <c>localhost:7040</c>
    /// <c>bookshop-api-xyxs.onrender.com</c>
    /// </remarks>
    public string PublicBaseUrl { get; init; } = string.Empty;
}
