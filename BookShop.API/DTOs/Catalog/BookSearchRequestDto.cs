using System.Security.Cryptography.X509Certificates;

namespace BookShop.API.DTOs.Catalog;
public sealed record BookSearchRequestDto(string SearchTerm, bool? IsAvailable);
