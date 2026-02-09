namespace BookShop.API.Infrastructure;

/// <summary>
/// Configuration wrapper for JWT-related settings.
/// Used for biniding configuration section that contains <see cref="JwtSettings"/>
/// </summary>
public class JwtServiceConfiguration
{
    /// <summary>
    /// JWT configuration values such as secret, issuer, audience and token lifetime.
    /// </summary>
    public JwtSettings JwtSettings { get; set; } = null!;
}

/// <summary>
/// Defines settings requered to generate and validate JWT access tokens.
/// </summary>
public class JwtSettings
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
