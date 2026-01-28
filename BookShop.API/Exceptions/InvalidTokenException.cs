namespace BookShop.API.Exceptions;

/// <summary>
/// Represents an authentication/authorization token validation failure.
/// </summary>
/// <remarks>
/// This exception is used for  cases where a security token is invalid, expired, mailformed, 
/// or cannot be verified (e.g. email confirmatio token, password reset token).
/// </remarks>
public class InvalidTokenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTokenException"/> class with a default message.
    /// </summary>
    public InvalidTokenException() : base("Invalid or expired token.") { }

    /// <summary>
    /// Initalizes a new instance of the <see cref="InvalidTokenException"/> class with a custom message.
    /// </summary>
    /// <param name="message">
    /// Error message describing the token failure.
    /// </param>
    public InvalidTokenException(string message) : base(message) { }
}
