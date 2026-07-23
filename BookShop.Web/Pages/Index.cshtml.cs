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
        throw new ArgumentException(nameof(bookService));

    /// <summary>
    /// Books displayed on the home page.
    /// </summary>
    public IReadOnlyCollection<BookDto> Books { get; private set; } = [];

    public bool IsApiAwakening { get; private set; } = false;

    /// <summary>
    /// Loads the list of books displayed on the home page.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
        catch
        {
            IsApiAwakening = true;
            Books = [];
        }
    }
}
