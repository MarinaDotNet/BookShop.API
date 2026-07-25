namespace BookShop.Web.DTOs.Shared;

public sealed record ApiProblemDetails(int? Status, string? Title, string? Detail, string? Instance);