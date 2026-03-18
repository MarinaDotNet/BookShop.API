using Asp.Versioning;
using BookShop.API.DTOs.Auth;
using BookShop.API.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace BookShop.API.Controllers.V1;

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
public class AuthController(AuthServices auth) : BaseApiController
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

    /// <summary>
    /// Refreshes the user's authentication token.
    /// </summary>
    /// <param name="refreshToken">
    /// The refresh token provided in the request body to obtain a new JWT token.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the request.
    /// </param>
    /// <returns>
    /// (200) <see cref="OkObjectResult"/> with the refreshed JWT token.
    /// </returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] LogoutDto refreshToken, CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await _auth.RefreshTokenAsync(refreshToken.RefreshToken, ip, userAgent, cancellationToken);
        return Ok(result); 
    }

    /// <summary>
    /// Revokes all active refresh tokens for the currently authenticated user, effectively logging the user out from the all devices.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the request.
    /// </param>
    /// <returns>
    /// Returns <see cref="StatusCodes.Status204NoContent"/> when the operation completes successfully.
    /// </returns>
    /// <remarks>
    /// This endpoint requires authentication and uses the current user's identity from the access token to revoke all active refresh tokens.
    /// </remarks>
    [HttpPost("lougout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await _auth.LogoutAllAsync(userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Initiates deletion of the currently authenticated user's account by validation the provided password and sending an account
    /// deletion confirmation link.
    /// </summary>
    /// <param name="dto">
    /// The request model containing the user's current password.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// <see cref="NoContentResult"/> when the deletion request is accepted and the confirmation email is sent. 
    /// </returns>
    /// <response code="204">
    /// The account deletion request was accepted and the confiramtion email was sent.
    /// </response>
    /// <response code="401">
    /// The user is not authorzed or provded inavlid credentials.
    /// </response>
    [HttpDelete("delete-account")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteUserAccount([FromBody] AccountDeleteDto dto, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _auth.RequestAccountDeletionAsync(userId, dto.Password, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Confirms account deletion using the token provided in the confirmation link and permanently marks the user account as deleted.
    /// </summary>
    /// <param name="token">
    /// The account deletion confirmation token from the query string.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A plain-text confirmation message when account deletion is completed successfully.
    /// </returns>
    /// <response code="200">
    /// The account deletion was confirmed successfully.
    /// </response>
    /// <response code="401">
    /// The provided token is invalid or expired.
    /// </response>
    /// <response code="404">
    /// The target user account was not found.
    /// </response>
    [HttpGet("confirm-account-deletion")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmAccountDeletion([FromQuery] string token, CancellationToken cancellationToken)
    {
        await _auth.ConfirmAccountDeletionAsync(token, cancellationToken);
        return Content("Account deletion confirmed successfully.", "text/plain");
    }

    /// <summary>
    /// Initiates recovery of the requested account by validating the provided email address and sending an account
    /// recovery confirmation link to the provided email address.
    /// </summary>
    /// <param name="dto">
    /// The request model contains the email address of the requested account recovery.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// <see cref="NoContentResult"/> when the recovery request is accepted and the confirmation email is sent. 
    /// </returns>
    /// <response code="204">
    /// The account recovery request was accepted and the confiramtion email was sent.
    /// </response>
    [HttpPost("recover-account")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RecoverAccount([FromBody] AccountRequestDto dto, CancellationToken cancellationToken)
    {
        await _auth.RequestAccountRecoveryAsync(dto.Email, cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    /// Confirms account recovery using the token provided in the confirmation link and marks the user account as undeleted.
    /// </summary>
    /// <param name="token">
    /// The account recovery confirmation token from the query string.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A plain-text confirmation message when account recovery is completed successfully.
    /// </returns>
    /// <response code="200">
    /// The account recovery was confirmed successfully.
    /// </response>
    /// <response code="401">
    /// The provided token is invalid or expired.
    /// </response>
    [HttpGet("confirm-account-recovery")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmAccountRecovery([FromQuery] string token, CancellationToken cancellationToken)
    {
        await _auth.ConfirmAccountRecoveryAsync(token, cancellationToken);
        return Content("Account recovery confirmed sucessfully.", "text/plain");
    }

    /// <summary>
    /// Updates the username of the currently authenticated user.
    /// </summary>
    /// <param name="dto">
    /// The request containing the new user name.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// <see cref="NoContentResult"/> when the username is updated successfully or when the requested username mathces the current one. 
    /// </returns>
    /// <response code="204">
    /// The username was updated successfully.
    /// </response>
    /// <response code="400">
    /// The request payload is invalid.
    /// </response>
    /// <response code="403">
    /// The user is not authenticated.
    /// </response>
    /// <response code="409">
    /// The requested username is already taken.
    /// </response>
    [HttpPatch("account/username")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameDto dto, CancellationToken cancellationToken)
    {
        int userId = GetCurrentUserId();
        await _auth.UpdateUsernameAsync(userId, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates the password of the currently authenticated user.
    /// </summary>
    /// <param name="dto">
    /// The request containing the current and new passwords.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancell the operation.
    /// </param>
    /// <returns>
    /// <see cref="NoContentResult"/> when the password is updated successfully. 
    /// </returns>
    /// <response code="204">
    /// The password was updated successfully.
    /// </response>
    /// <response code="400">
    /// The request payload is invalid.
    /// </response>
    /// <response code="401">
    /// The user is not authenticated.
    /// </response>
    /// <response code="403">
    /// The current user is not allowed to perform this action./
    /// </response>
    [HttpPatch("account/password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto, CancellationToken cancellationToken)
    {
        int userId = GetCurrentUserId();
        await _auth.UpdatePasswordAsync(userId, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates the account email address of the currently authenticated user.
    /// </summary>
    /// <param name="dto">
    /// The request containing current account password and new email address.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancell the operation.
    /// </param>
    /// <returns>
    /// <see cref="NoContentResult"/> when the email confirmation sent successfully. 
    /// </returns>
    /// <response code="204">
    /// The confirmation email was sent successfully.
    /// </response>
    /// <response code="400">
    /// The request payload is invalid.
    /// </response>
    /// <response code="401">
    /// The user is not authenticated.
    /// </response>
    /// <response code="403">
    /// The current user is not allowed to perform this action.
    /// </response>
    [HttpPatch("account/email")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailDto dto, CancellationToken cancellationToken)
    {
        int userId = GetCurrentUserId();
        await _auth.RequestEmailChangeAsync(userId, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Confirms email address update using the token provided in the confirmation link and marks the updated email address as confirmed.
    /// </summary>
    /// <param name="token">
    /// The email update confirmation token from the query string.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A plain-text confirmation message when email update is completed successfully.
    /// </returns>
    /// <response code="200">
    /// The email update was confirmed successfully.
    /// </response>
    /// <response code="401">
    /// The provided token is invalid or expired.
    /// </response>
    [HttpGet("confirm-email-change")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmUpdateEmail([FromQuery]string token, CancellationToken cancellationToken)
    {
        await _auth.ConfirmEmailChangeAsync(token, cancellationToken);
        return Content("Email address confirmed successfully.");
    }
}
