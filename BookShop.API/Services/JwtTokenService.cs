using BookShop.API.Infrastructure;
using BookShop.API.Models.Auth;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Data;

namespace BookShop.API.Services;

/// <summary>
/// Generates JSON web Tokens (JWT) for authenticated users.
/// </summary>
/// <remarks>
/// This service creates signed access tokens containing user identity and role claims. Token lifetime, issuer, audience, and secret key 
/// are configured via appliation settings.
/// </remarks>
public class JwtTokenService(IOptions<JwtServiceConfiguration> options) : IJwtTokenService
{
    private readonly JwtServiceConfiguration _options = options.Value;

    /// <summary>
    /// Creates a signed JWT access token for the specified user.
    /// </summary>
    /// <param name="user">
    /// The authenticated user form whom the token is generated.
    /// </param>
    /// <param name="roles">
    /// The collection of roles assigned to the user. Must contain at least one role.
    /// </param>
    /// <returns>
    /// A serialized JWT access token string 
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="roles"/> or <paramref name="user"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException ">
    /// Thrown when <paramref name="roles"/> is null.
    /// </exception>
    public string CreateAccessToken(User user, IReadOnlyCollection<Role> roles)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(roles, nameof(roles));
        if (roles.Count == 0)
        {
            throw new ArgumentException("User must have at least one role.", nameof(roles));
        }

        ValidateJwtSettingsConfiguration();

        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_options.JwtSettings.Secret);

        ClaimsIdentity subject = new(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ]);
        
        foreach( var role in roles)
        {
            subject.AddClaim(new Claim(ClaimTypes.Role, role.Name));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            Expires = DateTime.UtcNow.Add(_options.JwtSettings.TokenLifeTime),
            Issuer = _options.JwtSettings.Issuer,
            Audience = _options.JwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Validates the JWT configuration required for access token generation.
    /// </summary>
    /// <remarks>
    /// Ensure that issuer, audience, and secret key values are properly configured before generating a JWT.
    /// This method should be called prior to token creation prevent misconfigured authentication.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when one or more required JWT settings (Issuer, Audience, Secret) are missing or empty.
    /// </exception>
    private void ValidateJwtSettingsConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.JwtSettings.Issuer))
        {
            throw new InvalidOperationException("JwtSettings:Issuer is not configured.");
        }
        if (string.IsNullOrWhiteSpace(_options.JwtSettings.Audience))
        {
            throw new InvalidOperationException("JwtSettings:Audience is not configured.");
        }
        if (string.IsNullOrWhiteSpace(_options.JwtSettings.Secret))
        {
            throw new InvalidOperationException("JwtSettings:Secret is not configured.");
        }
    }
}
