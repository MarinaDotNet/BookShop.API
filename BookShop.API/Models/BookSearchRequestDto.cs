using System.Security.Cryptography.X509Certificates;

namespace BookShop.API.Models;
public sealed record BookSearchRequestDto(string SearchTerm, bool? IsAvailable);
