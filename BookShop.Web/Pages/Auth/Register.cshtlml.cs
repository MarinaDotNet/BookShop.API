using System.ComponentModel.DataAnnotations;
using BookShop.Web.DTOs.Auth;
using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookShop.Web.Pages.Auth;

/// <summary>
/// Represents the registration page model responsible for creating new user accounts.
/// </summary>
public sealed class RegisterModel(IAuthService authService) : PageModel
{
    private readonly IAuthService _authService = authService
        ?? throw new ArgumentException(nameof(authService));

    [BindProperty]
    public UserRegisterDto? Register { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string ConfirmationPassword {get; set;} = string.Empty;

    /// <summary>
    /// Processes the registration request and redirects the user to the information page when the registration request is successfully
    /// submitted.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancell the operation.
    /// </param>
    /// <returns>
    /// A page result when validation fails; otherwise, a redirect to the information page.
    /// </returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        ValidateRegistrationData();
        if (!ModelState.IsValid)
        {
            return Page();
        } 
        try
        {
            await _authService.RegisterAsync(Register!, cancellationToken);
            
            return RedirectToPage("/Auth/Information", new { 
                    title = "Registration Successful", 
                    message = "We've sent a confirmation to your email address.\n" +
                    "Please check your inbox and click the confirmation link to activate your account.\n" +
                    "If you don't see the email, check your Spam or Junk folder."});
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "Invalid email, password, or username.");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
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
        if (string.IsNullOrWhiteSpace(Register!.Password))
        {
            ModelState.AddModelError(nameof(Register.Password), "The password is required.");
        }
        if (string.IsNullOrWhiteSpace(ConfirmationPassword))
        {
            ModelState.AddModelError(nameof(ConfirmationPassword), "The confirmation password is required.");
        }
        if(Register!.Password != ConfirmationPassword)
        {
            ModelState.AddModelError(nameof(ConfirmationPassword), "The confirmation password does not match password.");
        }
    }
}