namespace BookShop.API.Infrastructure;

// <summary>
/// Defines settings requered to generate and validate JWT access tokens.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Symmetric secret key used to sign JWT tokens.
    /// Must be sufficiently long and kept secure.
    /// </summary>
    public string Secret { get; set; } = null!;

    /// <summary>
    /// Lifetime of the issued access token.
    /// </summary>
    public TimeSpan TokenLifeTime { get; set; }

    /// <summary>
    /// Token issuer identifier (iss claim).
    /// </summary>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Token audience identifier (aud claim).
    /// </summary>
    public string Audience { get; set; } = null!;
}
