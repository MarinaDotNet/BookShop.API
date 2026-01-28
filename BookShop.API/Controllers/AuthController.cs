using BookShop.API.DTOs.Auth;
using BookShop.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

/// <summary>
/// Provides authentication-related API endpoints (e.g. user registration, email confirmation and etc.).
/// </summary>
/// <remarks>
/// This controller delegates business logic to <see cref="AuthServices"/> and relies on centralized exception handling 
/// (e.g., <c>ExceptionHandlingMiddleware</c>) to translate domain/service exceptions into RFC 7807 ProblemDetails
/// 
/// Security notes:
/// - Registration access credentials and therefore must use <see cref="FromBodyAttribute"/> to avoid leaking sensitive data via query strings and logs.
/// - Email confirmation is performed via a link, so the token is provided as a query parameter.
/// </remarks>
/// <param name="auth"></param>
[ApiController]
[Route("auth")]
public class AuthController(AuthServices auth) : ControllerBase
{
    private readonly AuthServices _auth = auth;

    /// <summary>
    /// Confirms a user's email address using an email confirmation token.
    /// </summary>
    /// <param name="token">
    /// The email confirmation token provided in the confirmation link query string.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancelation token that can be used to cancel the request.
    /// </param>
    /// <returns>
    /// <see cref="NoContentResult"/> (204) when the email confirmation is completed successfully.
    /// </returns>
    [HttpGet("confirm-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token, CancellationToken cancellationToken)
    {
        await _auth.ConfirmEmailAsync(token, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="dto">
    /// The registration payload containing the username, email, and password.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancelation token that can be used to cancel the request.
    /// </param>
    /// <returns>
    /// (201) <see cref="CreatedResult"/> with the created users's identifier.
    /// </returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDto dto, CancellationToken cancellationToken)
    {
        int id = await _auth.RegisterUserAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(RegisterUser), new {id});
    }

    /// <summary>
    /// Registers a new administrator account
    /// </summary>
    /// <param name="dto">
    /// The registration payload containing the username, email, and password.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancelation token that can be used to cancel the request.
    /// </param>
    /// <returns>
    /// (201) <see cref="CreatedResult"/> with the created admin's identifier.
    /// </returns>
    [HttpPost("admin/register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterAdmin([FromBody] UserRegisterDto dto, CancellationToken cancellationToken)
    {
        int id = await _auth.RegisterAdminAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(RegisterAdmin), new { id });
    }
}
