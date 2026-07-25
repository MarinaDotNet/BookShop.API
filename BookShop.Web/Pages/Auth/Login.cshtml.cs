using BookShop.Web.DTOs.Auth;
using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookShop.Web.Pages.Auth;

/// <summary>
/// Handles user authentication using the BookShop API and signs the authenticated user into the web application using ASP.NET
/// Core cookie authentication.
/// </summary>
public sealed class LoginModel(IAuthService authService) : PageModel
{
    private readonly IAuthService _authService = authService
        ?? throw new ArgumentNullException(nameof(authService));

    /// <summary>
    /// Gets or sets the user login credentials submitted by the login form.
    /// </summary>
    [BindProperty]
    public UserLoginDto? Login { get; set; }

    /// <summary>
    /// Gets a value indicating whether the BookShop API is currently unavailable because it is waking up after a cold start.
    /// </summary>
    public bool IsApiAwakening { get; private set; } = false;

    /// <summary>
    /// Authenticates the user against the BookShop API. On success, creates an authenticated cookie and redirects to the home page.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the authentication operation.
    /// </param>
    /// <returns>
    /// Redirects to the home page if authentication succeeds; otherwise redisplays the login page with an appropriate error message.
    /// </returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if(User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }
        try
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }
            
            LoginResultDto loginResult = await _authService.LoginAsync(Login!, cancellationToken);

            ClaimsPrincipal claimsPrincipal = CreatePrincipal(loginResult);       
            AuthenticationProperties authProperties = CreateAuthenticationProperties(loginResult);

            authProperties.StoreTokens(
            [ new AuthenticationToken
                {
                    Name = "access_token",
                    Value = loginResult.AccessToken
                },
                new AuthenticationToken
                {
                    Name = "refresh_token",
                    Value = loginResult.RefreshToken
                }]);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);
            return RedirectToPage("/Index");
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
            // Render is waking up after a cold start or the API is unavailable.
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
        return Page();
    }

    /// <summary>
    /// Displays the login page for anonymous users. Authenticated users are redirected to the home page.
    /// </summary>
    /// <returns>
    /// The login page for anonymous users; otherwise a redirect to the home page.
    /// </returns>
    public IActionResult OnGet()
    {
        if(User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }
        return Page();
    }

    /// <summary>
    /// Creates a <see cref="ClaimsPrincipal"/> from the JWT access token returned by the BookShop API. 
    /// </summary>
    /// <param name="loginResult">
    /// Authentication result containing the access token.
    /// </param>
    /// <returns>
    /// A claims principal representing the authenticated user.
    /// </returns>
    private static ClaimsPrincipal CreatePrincipal(LoginResultDto loginResult)
    {
        JwtSecurityTokenHandler tokenHandler = new();
        JwtSecurityToken token = tokenHandler.ReadJwtToken(loginResult.AccessToken);
        List<Claim> claims = [.. token.Claims];
        ClaimsIdentity identity = new(
            claims, 
            CookieAuthenticationDefaults.AuthenticationScheme,
            JwtRegisteredClaimNames.UniqueName,
            ClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Creates authentication properties used when signing the user in.
    /// </summary>
    /// <param name="loginResult">
    /// Authentication result containing the access and refresh tokens.
    /// </param>
    /// <returns>
    /// Authentication properties containing cookie settings.
    /// </returns>
    private static AuthenticationProperties CreateAuthenticationProperties(LoginResultDto loginResult)
    {
        double cookieLifeTime = 30;

        return new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(cookieLifeTime)
        };
    }
}