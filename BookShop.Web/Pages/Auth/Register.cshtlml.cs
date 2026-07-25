using System.ComponentModel.DataAnnotations;
using BookShop.Web.DTOs.Auth;
using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace BookShop.Web.Pages.Auth;

/// <summary>
/// Represents the registration page model responsible for creating new user accounts.
/// </summary>
public sealed class RegisterModel(IAuthService authService) : PageModel
{
    private readonly IAuthService _authService = authService
        ?? throw new ArgumentNullException(nameof(authService));

    [BindProperty]
    public UserRegisterDto? Register { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string ConfirmationPassword {get; set;} = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the BookShop API is currently unavailable because it is waking up after a cold start.
    /// </summary>
    public bool IsApiAwakening { get; private set; } = false;

    /// <summary>
    /// Processes the registration request and redirects the user to the information page after successful registration.
    /// submitted.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A page result when validation fails; otherwise, a redirect to the information page.
    /// </returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if(User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }
        try
        {
            if(Register is null)
            {
                ModelState.AddModelError(string.Empty, "The registration data is required.");
                return Page();
            }

            ValidateRegistrationData();
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _authService.RegisterAsync(Register, cancellationToken);

            return RedirectToPage("/Auth/Information", new { 
                    title = "Registration Successful", 
                    message = "We've sent a confirmation to your email address.\n" +
                    "Please check your inbox and click the confirmation link to activate your account.\n" +
                    "If you don't see the email, check your Spam or Junk folder."});
        }
        catch(OperationCanceledException)
        {
            // Render cold start may cause the request to time out.
            IsApiAwakening = true;
        }
        catch(HttpRequestException ex) when (
            ex.StatusCode is HttpStatusCode.TooManyRequests ||
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
        return Page();
    }

    /// <summary>
    /// Displays the register page for anonymous users. Authenticated users are redirected to the home page.
    /// </summary>
    /// <returns>
    /// The register page for anonymous users; otherwise a redirect to the home page.
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
    /// Validates the registration data before submitting it to the authentication service.
    /// </summary>
    /// <remarks>
    /// Ensures that the password and confirmation password are provided and that both password values match.
    /// Any validation errors are added to the model state.
    /// </remarks>
    private void ValidateRegistrationData()
    {
        if(Register is null)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(Register.Password))
        {
            ModelState.AddModelError(nameof(Register.Password), "The password is required.");
        }
        if (string.IsNullOrWhiteSpace(ConfirmationPassword))
        {
            ModelState.AddModelError(nameof(ConfirmationPassword), "The confirmation password is required.");
        }
        if(Register.Password != ConfirmationPassword)
        {
            ModelState.AddModelError(nameof(ConfirmationPassword), "The passwords do not match.");
        }
    }
}