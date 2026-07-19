using BookShop.Web.DTOs.Auth;
using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookShop.Web.Pages.Auth;

/// <summary>
/// Handles user authentication using the BookShop API and signs the authencticated user into the web application using ASP.NET
/// Core cookie authentication.
/// </summary>
public sealed class LoginModel(IAuthService authService) : PageModel
{
    private readonly IAuthService _authService = authService
        ?? throw new ArgumentException(nameof(authService));

    /// <summary>
    /// Gets or sets the user login credentials submitted by the login form.
    /// </summary>
    [BindProperty]
    public UserLoginDto? Login { get; set; }

    /// <summary>
    /// Authenticates the user against the BookShop API. On success, creates an authenticated cookie and redirects to the home page.
    /// </summary>
    /// <param name="cancellationToken">
    /// 
    /// </param>
    /// <returns>
    /// Redirects to the home page if authentication succeeds; otherwise redisplays the login page with a validation error.
    /// </returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if(!ModelState.IsValid)
        {
            return Page();
        }
        try
        {
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
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");      
        }
        catch(ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
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
        JwtSecurityToken jwt = tokenHandler.ReadJwtToken(loginResult.AccessToken);
        List<Claim> claims = [.. jwt.Claims];
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
        return new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };
    }
}