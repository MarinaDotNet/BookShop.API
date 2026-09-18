using BookShop.Web.DTOs.Auth;
using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BookShop.Web.Pages.Auth;

[Authorize]
public sealed class IndexModel(IAuthService authService) : PageModel
{
    private readonly IAuthService _authService = authService
        ?? throw new ArgumentNullException(nameof(authService));

    public UserProfileDto? Account { get; private set; }

    public bool IsApiAwakening { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Account = await _authService.GetCurrentUserAsync(cancellationToken);    
        }
        catch (OperationCanceledException)
        {
            // Render cold start page may cause the request to time out.
            IsApiAwakening = true;
        }
        catch(HttpRequestException ex) when (
            ex.StatusCode is HttpStatusCode.TooManyRequests 
            || ex.InnerException is System.Net.Sockets.SocketException)
        {
            // Render is wakeing up after a cold start or the local API is unavalable.
            IsApiAwakening = true;
        }
        catch(HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            return RedirectToPage("/Auth/Login");
        }
        catch(HttpRequestException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return Page();
    }
}