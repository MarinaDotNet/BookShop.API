using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookShop.API.Controllers;

/// <summary>
/// Provides a common base class for API controllers. Contains shared conrtoller functionality used across different API versions.
/// </summary>
[ApiController]
public abstract class BaseApiController() : ControllerBase
{
    /// <summary>
    /// Retrieves the identifier of the currently authenticated user from the claims contained in the access token.
    /// </summary>
    /// <returns>
    /// The identifier of the authenticated user.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the user identifier claim is missing or invalid.
    /// </exception>
    protected int GetCurrentUserId()
    {
        var userIdClaim = (User.FindFirst(ClaimTypes.NameIdentifier)?.Value) 
            ?? throw new UnauthorizedAccessException("User identifier claim is missing");

        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user identifier.");
        }

        return userId;
    }
}