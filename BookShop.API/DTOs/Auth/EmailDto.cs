namespace BookShop.API.DTOs.Auth;

/// <summary>
/// Represents the data required to compose a plain text email message.
/// </summary>
/// <param name="To">The recipient's email address. Cannot be null or empty.</param>
/// <param name="Subject">The subject line of the email. Cannot be null; may be empty.</param>
/// <param name="TextBody">The plain text content of the email message. Cannot be null; may be empty.</param>
public sealed record EmailDto(string To, string Subject, string TextBody);
