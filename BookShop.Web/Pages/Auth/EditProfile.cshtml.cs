using BookShop.Web.DTOs.Auth;
using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BookShop.Web.Dtos.Auth;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;

namespace BookShop.Web.Pages.Auth;

[Authorize]
public sealed class EditProfileModel(IAuthService authService) : PageModel
{
    private readonly IAuthService _authService = authService
        ?? throw new ArgumentNullException(nameof(authService));

    public UserProfileDto? Account { get; private set; }
    public bool IsApiAwakening { get; private set; }

    [BindProperty]
    public UpdateUsernameDto NewUsername { get; set; } = new UpdateUsernameDto(string.Empty);

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if(User.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/Auth/Login");
        }
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

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if(User.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/Auth/Login");
        }
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _authService.UpdateUsernameAsync(NewUsername, cancellationToken);

            await ChangeUserIdentity();

            return RedirectToPage("/Auth/Index");
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
            ModelState.AddModelError(nameof(NewUsername), ex.Message);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return Page();
    }

    private async Task ChangeUserIdentity()
    {
        AuthenticateResult authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if(!authenticateResult.Succeeded 
        || authenticateResult.Principal?.Identity is not ClaimsIdentity identity 
        || authenticateResult.Properties is null)
        {
            throw new InvalidOperationException("The current authentication session could not be restored.");
        }

        Claim? usernameClaim = identity.FindFirst(JwtRegisteredClaimNames.UniqueName);

        if(usernameClaim is not null)
        {
            identity.RemoveClaim(usernameClaim);
        }

        identity.AddClaim(new Claim(JwtRegisteredClaimNames.UniqueName, NewUsername.NewUserName));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, 
            authenticateResult.Principal, 
            authenticateResult.Properties);
    }
}
