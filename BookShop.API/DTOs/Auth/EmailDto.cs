namespace BookShop.API.DTOs.Auth;

public sealed record EmailDto(string To, string Subject, string TextBody);
