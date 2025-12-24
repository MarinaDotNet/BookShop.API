namespace BookShop.API.Exceptions;

public sealed class ForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ForbidenException class with a default error message indicating that access
    /// is forbidden.
    /// </summary>
    public ForbiddenException() : base("Access is forbidden.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the ForbidenException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ForbiddenException(string message) : base(message)
    {
    }
}
