using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookShop.Web.Pages.Auth;

/// <summary>
/// Handles user logout requests. Revokes the refresh token through the API and sings the user out of the current authenticaiton session.
/// </summary>
/// <param name="authService"></param>
public sealed class LogoutModel(IAuthService authService) : PageModel
{
    private readonly IAuthService _authService = authService
        ?? throw new ArgumentException(nameof(authService));

    /// <summary>
    /// Processes the logout request and redirects the user to the home page after a successful sign-out.
    /// </summary>
    /// <returns>
    /// A redirect to the application's home page, regardless of whether the user was authenticated.
    /// </returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {

        var refreshToken = await HttpContext.GetTokenAsync("refresh_token");

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return RedirectToPage("/Index");
        }

        await _authService.LogOutAsync(refreshToken, cancellationToken);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToPage("/Index");
    }
}
