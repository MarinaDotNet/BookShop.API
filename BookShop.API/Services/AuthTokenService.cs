using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;
using static BookShop.API.Models.AuthTokens;

namespace BookShop.API.Services;

/// <summary> 
/// Provides functionality for creating and validating secure, purpose‑bound authentication tokens. 
/// Uses ASP.NET Core Data Protection to protect token payloads.
/// </summary>
/// <param name="dataProtectionProvider">
/// The data protection provider used to create token protectors.
/// </param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="dataProtectionProvider"/> is null.</exception>
public sealed class AuthTokenService(IDataProtectionProvider dataProtectionProvider) : IAuthTokenService
{
    private const string ProtectorPurpose = "BookShop.API.AuthTokenService.v1";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDataProtector _protector = dataProtectionProvider is null
        ? throw new ArgumentNullException(nameof(dataProtectionProvider))
        : dataProtectionProvider.CreateProtector(ProtectorPurpose);

    /// <summary>
    /// Creates a secure authentication token for the specified purpose. 
    /// </summary> 
    /// <param name="purpose">The intended purpose of the token.</param> 
    /// <param name="userId">The ID of the user the token belongs to. Must be positive.</param> 
    /// <param name="expiresAtUtc">The UTC expiration time of the token.</param> 
    /// <param name="Email">Optional new email address. Required when <paramref name="purpose"/> is <see cref="AuthTokenPurpose.EmailChange"/>.</param> 
    /// <returns>A Base64‑URL encoded protected token string.</returns> 
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userId"/> is zero or negative.</exception> 
    /// <exception cref="ArgumentException">Thrown when <paramref name="expiresAtUtc"/> is not UTC or when email is required but missing.</exception>
    public string CreateToken(AuthTokenPurpose purpose, int userId, DateTime expiresAtUtc, string? Email = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);

        if(expiresAtUtc.Kind != DateTimeKind.Utc) 
        {
            throw new ArgumentException("The expiresAtUtc parameter must be in UTC.", nameof(expiresAtUtc));
        }

        if(purpose == AuthTokenPurpose.EmailChange && string.IsNullOrWhiteSpace(Email)) 
        {
            throw new ArgumentException("The email parameter must be provided for email change tokens.", nameof(Email));
        }

        var payload = new AuthTokenPayload(
            UserId: userId,
            Purpose: purpose,
            ExpiresAtUtc: expiresAtUtc,
            NewEmail: Email
        );

        var json = JsonSerializer.Serialize(payload, JsonOptions);

        var protectedData = _protector.Protect(json);

        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(protectedData));
    }

    /// <summary> 
    /// Attempts to validate a token and extract its payload. 
    /// </summary> /// <param name="token">The encoded token string to validate.</param> 
    /// <param name="expectedPurpose">The expected purpose of the token.</param> 
    /// <param name="payload">When successful, contains the parsed token payload.</param> 
    /// <returns><c>true</c> if the token is valid; otherwise <c>false</c>.</returns>
    public bool TryValidateToken(string token, AuthTokenPurpose expectedPurpose, out AuthTokenPayload? payload)
    {
        payload = default!;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        if(!TryDecodeToken(token, out var protectedData))
        {
            return false; //invalid token format
        }

        if(!TryUnprotect(protectedData!, out var json))
        {
            return false; //invalid token protection
        }

        if(!TryDeserializePayload(json!, out var parsed))
        {
            return false; //invalid token payload
        }

        if(!IsValid(parsed!, expectedPurpose))
        {
            return false; //invalid token data
        }
        
        payload = parsed;
        return true;
    }

    /// <summary> 
    /// Attempts to Base64‑URL decode the token into protected data. 
    /// </summary>
    private static bool TryDecodeToken(string token, out string? protectedData)
    {
        protectedData = string.Empty;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var bytes = WebEncoders.Base64UrlDecode(token);
            protectedData = Encoding.UTF8.GetString(bytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary> 
    /// Attempts to unprotect the protected token data. 
    /// </summary>
    private bool TryUnprotect(string protectedData, out string? json)
    {
        json = string.Empty;

        try
        {
            json = _protector.Unprotect(protectedData);
            return true;
        }
        catch
        {
            return false;

        }
    }

    /// <summary> 
    /// Attempts to deserialize the JSON payload into an <see cref="AuthTokenPayload"/>. 
    /// </summary>
    private static bool TryDeserializePayload(string json, out AuthTokenPayload? payload)
    {
        payload = default!;

        try
        {
            payload = JsonSerializer.Deserialize<AuthTokenPayload>(json, JsonOptions);
            return payload is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary> 
    /// Validates the token payload against the expected purpose and expiration rules. 
    /// </summary>
    private static bool IsValid(AuthTokenPayload payload, AuthTokenPurpose purpose)
    {
        if(payload.Purpose != purpose)
        {
            return false;
        }
        
        if(payload.ExpiresAtUtc.Kind != DateTimeKind.Utc)
        {
            return false;
        }

        if (DateTime.UtcNow >= payload.ExpiresAtUtc)
        {
            return false;
        }

        return purpose switch
        {
            AuthTokenPurpose.EmailChange 
                => !string.IsNullOrWhiteSpace(payload.NewEmail),

            _ => true
        };
    }
}
