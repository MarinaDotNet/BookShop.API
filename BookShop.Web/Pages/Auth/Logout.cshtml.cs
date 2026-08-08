using System.Linq.Expressions;
using System.Net;
using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookShop.Web.Pages.Auth;

/// <summary>
/// Handles user logout requests. Revokes the refresh token through the API and signs the user out of the current 
/// authentication session.
/// </summary>
/// <param name="authService">
/// Provides authentication operations.
/// </param>
public sealed class LogoutModel(IAuthService authService) : PageModel
{
    private readonly IAuthService _authService = authService
        ?? throw new ArgumentNullException(nameof(authService));

    /// <summary>
    /// Gets a value indicating whether the BookShop API is currently unavailable because it is waking up after a cold start.
    /// </summary>
    public bool IsApiAwakening { get; private set; } = false;

    /// <summary>
    /// Processes the logout request and redirects the user to the home page after a successful sign-out.
    /// </summary>
    /// <returns>
    /// A redirect to the application's home page, regardless of whether the user was authenticated.
    /// </returns>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if(User.Identity?.IsAuthenticated == false)
        {
            return RedirectToPage("/Auth/Login");
        }

        var refreshToken = await HttpContext.GetTokenAsync("refresh_token");

        try
        {
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _authService.LogOutAsync(refreshToken, cancellationToken);
            }
        }
        catch(OperationCanceledException)
        {
            // Render cold start may cause the request to time out.
            IsApiAwakening = true;
        }
        catch(HttpRequestException ex) when (
            ex.StatusCode is HttpStatusCode.TooManyRequests||
            ex.InnerException is System.Net.Sockets.SocketException)
        {
            // Render is waking up after a cold start.
            IsApiAwakening = true;
        }
        catch(HttpRequestException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch(ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        finally
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        return RedirectToPage("/Index");   
    }
/*
    /// <summary>
    /// Displays the logout page for authenticated users. Anonymous users are redirected to the login page.
    /// </summary>
    /// <returns>
    /// The logout page for authenticated users; otherwise a redirect to the login page.
    /// </returns>
    public IActionResult OnGet()
    {
        if(User.Identity?.IsAuthenticated == false)
        {
            return RedirectToPage("/Auth/Login");
        }
        return Page();
    } */
}
