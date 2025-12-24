namespace BookShop.API.Exceptions;

/// <summary>
/// Represents errors that occur when one or more validation rules are not satisfied.
/// </summary>
/// <remarks>Use this exception to indicate that input data or object state does not meet required validation
/// criteria. This exception is typically thrown by validation logic to signal invalid or missing values.</remarks>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ValidationException class with a default error message indicating that one or
    /// more validation errors have occurred.
    /// </summary>
    public ValidationException() : base("One or more validation errors occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ValidationException(string message) : base(message)
    {
    }
}
