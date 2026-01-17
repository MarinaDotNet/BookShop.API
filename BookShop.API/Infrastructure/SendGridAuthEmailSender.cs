using BookShop.API.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;

namespace BookShop.API.Infrastructure;

/// <summary>
/// Provides an implementation of the IAuthEmailSender interface that sends authentication-related emails using the
/// SendGrid service.
/// </summary>
/// <remarks>This class sends emails for authentication workflows such as account deletion, password reset, email
/// confirmation, and sensitive account changes. It uses the SendGrid API and requires proper configuration of API keys
/// and sender details. If required configuration values are missing, an InvalidOperationException is thrown during
/// initialization. All email operations are performed asynchronously and will throw an exception if the SendGrid
/// service does not accept the email for delivery.</remarks>
/// <param name="configuration">The application configuration containing SendGrid API credentials and sender information. Must include valid values
/// for 'ApiKey', 'SendGrid:FromEmail', and 'SendGrid:FromName'. Cannot be null.</param>
public class SendGridAuthEmailSender : IAuthEmailSender
{
    private readonly EmailAddress _fromAddress;
    private readonly SendGridClient _sendGrid;

    public SendGridAuthEmailSender(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var apiKey = configuration["SendGrid:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("SendGrid:APIKey is not configured.");
        }

        var fromEmail = configuration["SendGrid:FromEmail"];
        if(string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException("SendGrid:FromEmail is not configured.");
        }
        
        var fromName = configuration["SendGrid:FromName"];
        if (string.IsNullOrWhiteSpace(fromName))
        {
            throw new InvalidOperationException("SendGrid:FromName is not configured.");
        }

        _sendGrid = new SendGridClient(apiKey);
        _fromAddress = new EmailAddress(fromEmail, fromName);
    }

    /// <summary>
    /// Sends an account deletion confirmation email to the specified recipient with a link to confirm the deletion
    /// request.
    /// </summary>
    /// <remarks>The email includes both plain text and HTML content with instructions for confirming account
    /// deletion. If the recipient did not request account deletion, they are advised to ignore the email.</remarks>
    /// <param name="toEmail">The email address of the recipient who will receive the account deletion confirmation. Cannot be null, empty, or
    /// whitespace.</param>
    /// <param name="deletionLink">The URI containing the confirmation link for account deletion. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the email sending operation.</param>
    /// <returns>A task that represents the asynchronous operation of sending the confirmation email.</returns>
    public async Task SendAccountDeletionConfirmationAsync(string toEmail, Uri deletionLink, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentNullException.ThrowIfNull(deletionLink, nameof(deletionLink));

        EmailAddress toAddress = new(toEmail);

        string subject = "Confirm your account deletion";
        string plainText = $"Confirm your account deletion: {deletionLink}";
        string htmlContent = $"""
            <p>Please confirm your account deletion by clicking the link below:</p>
            <p><a href="{deletionLink}">Confirm Account Deletion</a></p>
            <p> If you did not request this, please ignore this email.</p>
            <p>Thank you!</p>
            <p>BookShop Team</p>
            <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
            <p>{deletionLink}</p>
            """;

       await SendEmailAsync(toAddress, subject, plainText, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends an email containing a confirmation link to verify an email address change request.
    /// </summary>
    /// <remarks>The confirmation email includes both plain text and HTML content with instructions for the
    /// recipient. If the recipient did not request an email change, they are advised to ignore the message.</remarks>
    /// <param name="toEmail">The recipient's email address to which the confirmation message will be sent. Cannot be null, empty, or consist
    /// only of white-space characters.</param>
    /// <param name="confirmationLink">A URI representing the confirmation link that the recipient must follow to confirm the email address change.
    /// Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous operation of sending the confirmation email.</returns>
    public async Task SendEmailChangeConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentNullException.ThrowIfNull(confirmationLink, nameof(confirmationLink));

        EmailAddress toAddress = new(toEmail);

        string subject = "Confirm your email change";
        string plainText = $"Confirm your email change: {confirmationLink}";
        string htmlContent = $"""
            <p>Please confirm your email change by clicking the link below:</p>
            <p><a href="{confirmationLink}">Confirm Email Change</a></p>
            <p> If you did not request this, please ignore this email.</p>
            <p>Thank you!</p>
            <p>BookShop Team</p>
            <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
            <p>{confirmationLink}</p>
            """;

       await SendEmailAsync(toAddress, subject, plainText, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends an email containing a confirmation link to the specified email address asynchronously.
    /// </summary>
    /// <param name="toEmail">The recipient's email address. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <param name="confirmationLink">The URI to be included in the email for confirming the recipient's email address. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the email sending operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendEmailConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentNullException.ThrowIfNull(confirmationLink, nameof(confirmationLink));

        EmailAddress toAddress = new(toEmail);

        string subject = "Confirm your email";
        string plainText = $"Confirm your email: {confirmationLink}";
        string htmlContent = $"""
            <p>Please confirm your email by clicking the link below:</p>
            <p><a href="{confirmationLink}">Confirm Email</a></p>
            <p> If you did not request this, please ignore this email.</p>
            <p>Thank you!</p>
            <p>BookShop Team</p>
            <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
            <p>{confirmationLink}</p>
            """;

        await SendEmailAsync(toAddress, subject, plainText, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends a password reset email to the specified recipient with a link to reset their password.
    /// </summary>
    /// <param name="toEmail">The email address of the recipient who will receive the password reset email. Cannot be null, empty, or consist
    /// only of white-space characters.</param>
    /// <param name="resetLink">The URI containing the password reset link to be included in the email. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendPasswordResetAsync(string toEmail, Uri resetLink, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentNullException.ThrowIfNull(resetLink, nameof(resetLink));

        EmailAddress toAddress = new(toEmail);

        string subject = "Reset your password";
        string plainText = $"Reset your password: {resetLink}";
        string htmlContent = $"""
            <p>You can reset your password by clicking the link below:</p>
            <p><a href="{resetLink}">Reset Password</a></p>
            <p> If you did not request this, please ignore this email.</p>
            <p>Thank you!</p>
            <p>BookShop Team</p>
            <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
            <p>{resetLink}</p>
            """;

        await SendEmailAsync(toAddress, subject, plainText, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends an email to the specified address containing a confirmation link for a sensitive account change, such as username.
    /// </summary>
    /// <remarks>This method is typically used to verify user-initiated sensitive changes and helps prevent
    /// unauthorized modifications. The email includes both HTML and plain text content for compatibility with various
    /// email clients.</remarks>
    /// <param name="toEmail">The recipient's email address to which the confirmation message will be sent. Cannot be null, empty, or consist
    /// only of white-space characters.</param>
    /// <param name="confirmationLink">The URI containing the confirmation link that the recipient must follow to confirm the sensitive change. Cannot
    /// be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the email sending operation.</param>
    /// <returns>A task that represents the asynchronous operation of sending the confirmation email.</returns>
    public async Task SendSensitiveChangeConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(confirmationLink, nameof(confirmationLink));
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail, nameof(toEmail));

        EmailAddress toAddress = new(toEmail);

        string subject = "Confirm your sensitive change";
        string plainText = $"Confirm your sensitive change: {confirmationLink}";
        string htmlContent = $"""
            <p>Please confirm your sensitive change by clicking the link below:</p>
            <p><a href="{confirmationLink}">Confirm Sensitive Change</a></p>
            <p> If you did not request this, please ignore this email.</p>
            <p>Thank you!</p>
            <p>BookShop Team</p>
            <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
            <p>{confirmationLink}</p>
            """;

       await SendEmailAsync(toAddress, subject, plainText, htmlContent, cancellationToken);

    }

    /// <summary>Sends an email using SendGrid.</summary>
    /// <param name="toAddress">The recipient's email address.</param>
    /// <param name="subject">The subject of the email. </param>
    /// <param name="plainTextContent">The plain text content of the email. </param>
    /// <param name="htmlContent">The HTML content of the email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task representing the asynchronous operation. </returns>
    /// <exception cref="InvalidOperationException">An error occurred while sending the email.</exception>
    private async Task SendEmailAsync(EmailAddress toAddress, string subject, string plainTextContent, string htmlContent, CancellationToken cancellationToken)
    {
        var message = MailHelper.CreateSingleEmail(_fromAddress, toAddress, subject, plainTextContent, htmlContent);
        var response = await _sendGrid.SendEmailAsync(message, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            throw new InvalidOperationException($"Failed to send email to {toAddress.Email}. Status Code: {response.StatusCode}");
        }
    }
}
