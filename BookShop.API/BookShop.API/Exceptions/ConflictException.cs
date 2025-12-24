namespace BookShop.API.Exceptions;

public sealed class ConflictException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ConflictException class with a default error message indicating that a
    /// conflict has occurred.
    /// </summary>
    public ConflictException() : base("A conflict has occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConflictException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConflictException(string message) : base(message)
    {
    }
}
