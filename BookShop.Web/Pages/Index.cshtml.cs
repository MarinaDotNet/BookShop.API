using System.Net;
using BookShop.Web.DTOs.Catalog;
using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookShop.Web.Pages;

/// <summary>
/// Represents the home page.
/// </summary>
public sealed class IndexModel(IBookService bookService) : PageModel
{
    private readonly IBookService _bookService = bookService ??
        throw new ArgumentNullException(nameof(bookService));

    /// <summary>
    /// Books displayed on the home page.
    /// </summary>
    public IReadOnlyCollection<BookDto> Books { get; private set; } = [];

    /// <summary>
    /// Gets a value indicating whether the BookShop API is currently unavailable because it is waking up after a cold start.
    /// </summary>
    public bool IsApiAwakening { get; private set; } = false;

    /// <summary>
    /// Loads the list of books displayed on the home page.
    /// </summary>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Books = await _bookService.GetTopCheapestBooksAsync(10, cancellationToken);

            if(Books.Count == 0)
            {
                IsApiAwakening = true;
            }
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
    }
}
