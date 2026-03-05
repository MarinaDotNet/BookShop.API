using Asp.Versioning;
using BookShop.API.DTOs.Auth;
using BookShop.API.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
[EnableCors("PublicPolicy")]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/auth")]
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
    [AllowAnonymous]
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
    [AllowAnonymous]
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
    [EnableCors("AdminPolicy")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] UserRegisterDto dto, CancellationToken cancellationToken)
    {
        int id = await _auth.RegisterAdminAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(RegisterAdmin), new { id });
    }

    /// <summary>
    /// Resends the email confirmation link.
    /// </summary>
    /// <param name="dto">
    /// The resend email confirmation payload containing the email.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancelation token that can be used to cancel the request.
    /// </param>
    /// <returns>
    /// (204) <see cref="NoContentResult"/> when the resend operation is completed successfully.
    /// </returns>
    [HttpPost("resend-email-confirmation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationDto dto, CancellationToken cancellationToken)
    {
        await _auth.ResendEmailConfirmationLink(dto.Email, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Authenticates a user and issues a JWT token upon successful login.
    /// </summary>
    /// <param name="dto">
    /// The login payload containing the username and password.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancelation token that can be used to cancel the request.
    /// </param>
    /// <returns>
    /// (200) <see cref="OkObjectResult"/> with the issued JWT token.
    /// </returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto, CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await _auth.LoginAsync(dto, ip, userAgent, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Logs out the user and invalidates their refresh token.
    /// </summary>
    /// <param name="logoutDto">
    /// The logout payload containing the refresh token to be invalidated.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the request.
    /// </param>
    /// <returns>
    /// (204) <see cref="NoContentResult"/> when the logout operation is completed successfully.
    /// </returns>
    [HttpPost("logout")]
    [Authorize(Roles = "user, admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto, CancellationToken cancellationToken)
    {
        await _auth.LogoutAsync(logoutDto.RefreshToken, cancellationToken);
        return NoContent();
    }
}
