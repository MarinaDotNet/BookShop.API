namespace BookShop.API.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a requested resource cannot be found.
/// </summary>
/// <remarks>Use this exception to indicate that an operation failed because the specified resource does not
/// exist. This exception is typically used in scenarios such as data retrieval or lookup operations where the absence
/// of a resource is not considered a system error but should be communicated to the caller.</remarks>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the NotFoundException class with a default error message indicating that a
    /// requested resource was not found.
    /// </summary>
    public NotFoundException() : base("Resource not found")
    {
    }

    /// <summary>
    /// Initializes a new instance of the NotFoundException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotFoundException(string message) : base(message)
    {
    }
}
