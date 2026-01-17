using SendGrid.Helpers.Mail;

namespace BookShop.API.Services;

/// <summary>
/// Defines methods for sending authentication-related emails, such as confirmation, password reset, and account change
/// notifications, to users.
/// </summary>
/// <remarks>Implementations of this interface are responsible for delivering emails that facilitate user
/// authentication workflows, including confirming email addresses, resetting passwords, and notifying users of
/// sensitive account changes. Methods are asynchronous and accept a cancellation token to support cancellation of email
/// delivery operations. This interface does not prescribe the underlying email transport or formatting; implementations
/// may vary depending on application requirements.</remarks>
public interface IAuthEmailSender
{
    /// <summary>
    /// Sends an email message containing a confirmation link to the specified email address asynchronously.
    /// </summary>
    /// <param name="toEmail">The recipient's email address to which the confirmation message will be sent. Cannot be null.</param>
    /// <param name="confirmationLink">The URI to be included in the email for confirming the recipient's email address. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the email sending operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendEmailConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a password reset email to the specified recipient with a link to reset their password.
    /// </summary>
    /// <param name="toEmail">The email address of the recipient who will receive the password reset email. Cannot be null.</param>
    /// <param name="resetLink">The URI containing the password reset link to be included in the email. Must be an absolute URI.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendPasswordResetAsync(string toEmail, Uri resetLink, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an email containing a confirmation link to verify a change of email address.
    /// </summary>
    /// <param name="toEmail">The email address to which the confirmation link will be sent. Cannot be null.</param>
    /// <param name="confirmationLink">The URI of the confirmation link that the recipient must follow to confirm the email address change. Cannot be
    /// null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendEmailChangeConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an account deletion confirmation email to the specified recipient with a link to confirm the deletion.
    /// </summary>
    /// <remarks>The email will include the provided deletion link, allowing the recipient to confirm the
    /// account deletion process. This method does not guarantee delivery; it only initiates the send
    /// operation.</remarks>
    /// <param name="toEmail">The email address of the recipient who will receive the account deletion confirmation message. Cannot be null.</param>
    /// <param name="deletionLink">A URI containing the link that the recipient can use to confirm account deletion. Must be an absolute URI.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendAccountDeletionConfirmationAsync(string toEmail, Uri deletionLink, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an email containing a confirmation link to verify a sensitive account change, such as an email address or
    /// password update.
    /// </summary>
    /// <remarks>This method does not complete until the confirmation email has been sent or the operation is
    /// canceled. The confirmation link should be unique and time-limited to ensure security.</remarks>
    /// <param name="toEmail">The email address of the recipient who will receive the confirmation message. Cannot be null.</param>
    /// <param name="confirmationLink">The URI that the recipient must visit to confirm the sensitive change. Must be a valid, absolute URI.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendSensitiveChangeConfirmationAsync(string toEmail, Uri confirmationLink, CancellationToken cancellationToken);

}
