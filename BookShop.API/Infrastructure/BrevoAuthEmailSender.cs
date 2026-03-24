using BookShop.API.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace BookShop.API.Infrastructure;

/// <summary>
/// Provides an implementation of the IAuthEmailSender interface that sends authentication-related emails using the
/// Brevo service.
/// </summary>
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

    /// <summary>
    /// Sends an account deletion confirmation email to the specified recipient with a link to confirm the deletion request.
    /// </summary>
    /// <remarks>
    /// The email includes both plain text and HTML content with instructions for confirming account
    /// deletion. If the recipient did not request account deletion, they are advised to ignore the email.
    /// </remarks>
    /// <param name="toEmail">
    /// The email address of the recipient who will receive the account deletion confirmation. Cannot be null, empty, or
    /// whitespace.
    /// </param>
    /// <param name="deletionLink">
    /// The URI containing the confirmation link for account deletion. Cannot be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the email sending operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation of sending the confirmation email.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <see cref="ArgumentNullException"/> thrown if the <paramref name="deletionLink"/> is null. </exception>
    public async Task SendAccountDeletionConfirmationAsync(string toEmail, Uri deletionLink, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(deletionLink, nameof(deletionLink));

        string subject = "Confirm your account deletion";
        string textContent = $"Confirm your account deletion: {deletionLink}";
        string htmlContent = $"""
            <p>Please confirm your account deletion by clicking the link below:</p>
            <p><a href="{deletionLink}">Confirm Account Deletion</a></p>
            <p>If you did not request this, please ignore this email.</p>
            <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
            <p>{deletionLink}</p>
            <p>Thank you,<br/>The BookShop Team</p>
            """;

        await SendAsync(toEmail, subject, textContent, htmlContent, cancellationToken);

    }

    /// <summary>
    /// Sends an email containing a confirmation link to verify an email address change request.
    /// </summary>
    /// <remarks>
    /// The confirmation email includes both plain text and HTML content with instructions for the
    /// recipient. If the recipient did not request an email change, they are advised to ignore the message.
    /// </remarks>
    /// <param name="toEmail">
    /// The recipient's email address to which the confirmation message will be sent. Cannot be null, empty, or consist
    /// only of white-space characters.
    /// </param>
    /// <param name="confirmationLink">
    /// A URI representing the confirmation link that the recipient must follow to confirm the email address change.
    /// Cannot be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the send operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation of sending the confirmation email.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <see cref="ArgumentNullException"/> thrown if the <paramref name="confirmationLink"/> is null. </exception>
    public async Task SendEmailChangeConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(confirmationLink, nameof(confirmationLink));

        string subject = "Please confirm your email change";
        string textContent = $"Confirm your email change: {confirmationLink}";
        string htmlContent = $"""
            <p>Please confirm your email change by clicking the link below:</p>
            <p><a href="{confirmationLink}">Confirm Email Change</a></p>
            <p>If you did not request this, please ignore this email.</p>
            <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
            <p>{confirmationLink}</p>
            <p>Thank you,<br/>The BookShop Team</p>
            """;

        await SendAsync(toEmail, subject, textContent, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends an email containing a confirmation link to the specified email address asynchronously.
    /// </summary>
    /// <param name="toEmail">
    /// The recipient's email address. Cannot be null, empty, or consist only of white-space characters.
    /// </param>
    /// <param name="confirmationLink">
    /// The URI to be included in the email for confirming the recipient's email address. Cannot be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the email sending operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <see cref="ArgumentNullException"/> thrown if the <paramref name="confirmationLink"/> is null. </exception>
    public async Task SendEmailConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(confirmationLink, nameof(confirmationLink));

        string subject = "Please confirm your email address";
        string textContent = $"Confirm your email by clicking the following link: {confirmationLink}";
        string htmlContent = $"""
            <p>Please confirm your email by clicking the following link:</p>
            <p><a href="{confirmationLink}">Confirm Email</a></p>
            <p>If you did not request this, please ignore this email.</p>
            <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
            <p>{confirmationLink}</p>
            <p>Thank you,<br/>The BookShop Team</p>
            """;

        await SendAsync(toEmail, subject, textContent, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends a password reset email to the specified recipient with a link to reset their password.
    /// </summary>
    /// <param name="toEmail">
    /// The email address of the recipient who will receive the password reset email. Cannot be null, empty, or consist
    /// only of white-space characters.
    /// </param>
    /// <param name="resetLink">
    /// The URI containing the password reset link to be included in the email. Cannot be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <see cref="ArgumentNullException"/> thrown if the <paramref name="resetLink"/> is null. </exception>
    public async Task SendPasswordResetAsync(string toEmail, Uri resetLink, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resetLink, nameof(resetLink));

        string subject = "Reset your password";
        string textContent = $"Reset your password:: {resetLink}";
        string htmlContent = $"""
            <p>You can reset your password by clicking the link below:</p>
            <p><a href="{resetLink}">Reset Password</a></p>
            <p>If you did not request this, please ignore this email.</p>
            <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
            <p>{resetLink}</p>
            <p>Thank you,<br/>The BookShop Team</p>
            """;

        await SendAsync(toEmail, subject, textContent, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends an email to the specified address containing a confirmation link for a sensitive account change, such as username.
    /// </summary>
    /// <remarks>
    /// This method is typically used to verify user-initiated sensitive changes and helps prevent
    /// unauthorized modifications. The email includes both HTML and plain text content for compatibility with various
    /// email clients.
    /// </remarks>
    /// <param name="toEmail">
    /// The recipient's email address to which the confirmation message will be sent. Cannot be null, empty, or consist
    /// only of white-space characters.
    /// </param>
    /// <param name="confirmationLink">
    /// The URI containing the confirmation link that the recipient must follow to confirm the sensitive change. Cannot
    /// be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the email sending operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation of sending the confirmation email.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <see cref="ArgumentNullException"/> thrown if the <paramref name="confirmationLink"/> is null. </exception>
    public async Task SendSensitiveChangeConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(confirmationLink, nameof(confirmationLink));

        string subject = "Confirm your sensitive change";
        string textContent = $"Confirm your sensitive change: {confirmationLink}";
        string htmlContent = $"""
            <p>Please confirm your sensitive change by clicking the link below:</p>
            <p><a href="{confirmationLink}">Confirm Sensitive Change</a></p>
            <p>If you did not request this, please ignore this email.</p>
            <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
            <p>{confirmationLink}</p>
            <p>Thank you,<br/>The BookShop Team</p>
            """;

        await SendAsync(toEmail, subject, textContent, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends an email to the specified address containing a confirmation link for a account recovery for account that was deleted.
    /// </summary>
    /// <remarks>
    /// This method is typically used to verify user-initiated sensitive changes and helps prevent
    /// unauthorized modifications. The email includes both HTML and plain text content for compatibility with various
    /// email clients.
    /// </remarks>
    /// <param name="toEmail">
    /// The recipient's email address to which the confirmation message will be sent. Cannot be null, empty, or consist
    /// only of white-space characters.
    /// </param>
    /// <param name="confirmationLink">
    /// The URI containing the confirmation link that the recipient must follow to confirm the account recovery. Cannot
    /// be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the email sending operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation of sending the confirmation email.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <see cref="ArgumentNullException"/> thrown if the <paramref name="confirmationLink"/> is null. </exception>
    public async Task SendAccountRecoveryConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(confirmationLink, nameof(confirmationLink));

        string subject = "Confirm account recovery";
        string textContent = $"Confirm account recovery: {confirmationLink}";
        string htmlContent = $"""
        <p>Please confirm account recovery by clicking the link below:</p>
        <p><a href="{confirmationLink}">Confirm Account Recovery</a></p>
        <p>If you did not request this, please ignore the email.</p>
        <p>If the link doesn't work, copy and paste the following URL into your browser:</p>
        <p>{confirmationLink}</p>
        <p>Thank you,<br/>The BookShop Team</p>
        """;

        await SendAsync(toEmail, subject, textContent, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends a password change notification email to the specified recepient.
    /// </summary>
    /// <param name="toEmail">
    /// The recepient email address.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancell the operation.
    /// </param>
    /// <returns>
    /// A task that represents an asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="toEmail"/> is null, empty, or consists only of white space characters.
    /// </exception>
    public async Task SendPasswordChangedAsync(string toEmail, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(toEmail, nameof(toEmail));

        string subject = "Your password has been changed";
        string textContent = @"Your password has been successfully changed.

        If you did not perform this action, please reset your password immediately or contact support.

        Thank you,
        BookShop Team";

        string htmlContent = $@"
        <p>Your password has been successfully changed.</p>
        <p>If you did not perform this action, please reset your password immediately or contact support.</p>
        <br/>
        <p>Thank you,<br/>BookShop Team</p>";

        await SendAsync(toEmail, subject, textContent, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends a security notification email to the previous and new email addresses after the account email has 
    /// been changed successfully.
    /// </summary>
    /// <param name="oldEmail">
    /// The previous email address associated with the account. This address receives the notification to 
    /// help detect unauthorized changes.
    /// </param>
    /// <param name="newEmail">
    /// The new email address associated with the account. This address recieves the notification 
    /// confirming that the change was completed.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancell the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="oldEmail"/> or <paramref name="newEmail"/> is null, empty, or consists only 
    /// of white space characters.
    /// </exception>
    public async Task SendEmailChangedAsync(string oldEmail, string newEmail, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(oldEmail, nameof(oldEmail));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(newEmail, nameof(newEmail));

        string subject = "Your email address has been changed";

        var dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");

        string textContent = @$"Your email address has been successfully changed.
        
        If you did not perform this action, please reset your password immediately or contact support.

        Time: {dateTime} UTC

        - BookShop Team";

        string htmlContent = $@"
        <p>Your email address has been successfully changed.</p>
        
        <p>If you did not perform this action, please <b>reset your password immediately</b> or conatct support.</p>
        
        <p>Time: {dateTime} UTC</p>
        
        <br/>
        <p>- BookShop</p>";

        await SendAsync(oldEmail, subject, textContent, htmlContent, cancellationToken);
        await SendAsync(newEmail, subject, textContent, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends a security notification email confirming that the account has been deleted.
    /// </summary>
    /// <param name="email">
    /// The email address that should receive the deltion notification.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancell the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="email"/> is null, empty, or consists only of white space characters.
    /// </exception>
    public async Task SendAccountDeletedAsync(string email, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(email, nameof(email));

        string subject = "Your account has been deleted";

        var dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");

        string textContent = @$"Your account has been successfully deleted.
        
        If you did not perform this action, please contact support immediately.

        Time: {dateTime} UTC

        - BookShop Team";

        string htmlContent = $@"
        <p>Your account has been successfully deleted.</p>
        
        <p>If you did not perform this action, please <b>contact support immediately</b>.</p>
        
        <p>Time: {dateTime} UTC</p>
        
        <br/>
        <p>- BookShop</p>";

        await SendAsync(email, subject, textContent, htmlContent, cancellationToken);
    }

    /// <summary>
    /// Sends a security notification email confirming that the account has been recovered successfully.
    /// </summary>
    /// <param name="email">
    /// The email address that should receive the recovery notification.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancell the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="email"/> is null, empty, or consists only of white space characters.
    /// </exception>
    public async Task SendAccountRecoveredAsync(string email, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(email, nameof(email));

        string subject = "Your account has been restored";

        var dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");

        string textContent = @$"Your account has been successfully restored.
        
        If you did not perform this action, please reset you password immediately.

        Time: {dateTime} UTC

        - BookShop Team";

        string htmlContent = $@"
        <p>Your account has been successfully restored.</p>
        
        <p>If you did not perform this action, please <b>reset you password immediately</b>.</p>
        
        <p>Time: {dateTime} UTC</p>
        
        <br/>
        <p>- BookShop</p>";

        await SendAsync(email, subject, textContent, htmlContent, cancellationToken);
    }
    
    /// <summary>
    /// Sends an email using http client.
    /// </summary>
    /// <param name="toEmail">
    /// The recipient's email address.
    /// </param>
    /// <param name="subject">
    /// The subject of the email.
    /// </param>
    /// <param name="textContent">
    /// The plain text content of the email.
    /// </param>
    /// <param name="htmlContent">
    /// The HTML content of the email.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// The task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// An error occurred while sending the email.
    /// </exception>
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
