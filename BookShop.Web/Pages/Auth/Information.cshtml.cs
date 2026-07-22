using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookShop.Web.Pages.Auth;

/// <summary>
/// Represents a page model used to display information messages to the user.
/// </summary>
public sealed class InformationModel : PageModel
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Initializes the information page with the specified title and message.
    /// </summary>
    /// <param name="title">
    /// The page title. if <see langword="null"/>, the default title is used.
    /// </param>
    /// <param name="message">
    /// The informational message to display.
    /// </param>
    public void OnGet(string? title, string? message)
    {
        Title = title ?? "Information";
        Message = message ?? string.Empty;
    }
}