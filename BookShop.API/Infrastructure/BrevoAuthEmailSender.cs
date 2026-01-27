using BookShop.API.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace BookShop.API.Infrastructure;

public class BrevoAuthEmailSender : IAuthEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly BrevoOptions _options;

    public BrevoAuthEmailSender(HttpClient httpClient, IOptions<BrevoOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.FromEmail))
            throw new InvalidOperationException("Brevo:FromEmail is not configured.");
        if (string.IsNullOrWhiteSpace(_options.FromName))
            throw new InvalidOperationException("Brevo:FromName is not configured.");
    }
    public Task SendAccountDeletionConfirmationAsync(string toEmail, Uri deletionLink, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SendEmailChangeConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task SendEmailConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(confirmationLink, nameof(confirmationLink));

        string subject = "Please confirm your email address";
        string plainText = $"Confirm your email by clicking the following link: {confirmationLink}";
        string htmlContent = $"""
            <p>Please confirm your email by clicking the following link:</p>
            <p><a href="{confirmationLink}">Confirm Email</a></p>
            <p>If you did not request this, please ignore this email.</p>
            <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
            <p>{confirmationLink}</p>
            <p>Thank you,<br/>The BookShop Team</p>
            """;

        await SendAsync(toEmail, subject, plainText, htmlContent, cancellationToken);

    }

    public Task SendPasswordResetAsync(string toEmail, Uri resetLink, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SendSensitiveChangeConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task SendAsync(string toEmail, string subject, string textContent, string htmlContent, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(htmlContent);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(textContent);

        using var request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email");

        var payload = new
        {
            sender = new
            {
                name = _options.FromName,
                email = _options.FromEmail
            },
            to = new[]
            {
                new { email = toEmail }
            },
            subject,
            textContent,
            htmlContent
        };
        request.Content = JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Brevo send failed ({(int)response.StatusCode} {response.ReasonPhrase}). Body: {body}");
        }
    }
}
