using BookShop.API.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace BookShop.API.Middleware;

/// <summary>
/// Builds RFC 7807 <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> responces for unhandled exceptions.
/// </summary>
/// <remarks>
/// This builder centralizes exception-to-HTTP mapping for the whole API (Auth, Books, and etc.). It is intended to be used by <c>ExceptionHandlingMiddleware</c>
/// to convert exceptions thrown from controllers/services/repositories into consistent <c>applciation/json</c> responses.
/// </remarks>
public static class ProblemDetailsBuilder
{
    /// <summary>
    /// Creates a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> instance for the given exception.
    /// </summary>
    /// <param name="context">
    /// The current HTTP context. Used to populate <see cref="ProblemDetails.Instance"/>
    /// </param>
    /// <param name="exception">
    /// The exception to convert into <see cref="ProblemDetails"/> responce.
    /// </param>
    /// <returns>
    /// A populated <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> instance with a HTTP status code, title, detail and instance.
    /// </returns>
    public static ProblemDetails Build(HttpContext context, Exception exception)
    {
        var status = ResolveStatusCode(exception);

        var title = ResolveTitle(exception, status);

        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError
            ? "An unexpected error occured."
            : exception.Message,
            Instance = context.Request.Path
        };
    }

    /// <summary>
    /// Maps the given <paramref name="exception"/> to an HTTP status code for a <c>ProblemDetails</c> response.
    /// </summary>
    /// <param name="exception">The exception to map. Cannot be null.</param>
    /// <returns>
    /// The HTTP status code that should be returned to the client.
    /// </returns>
    private static int ResolveStatusCode(Exception exception) =>
        exception switch
        {
            //400 BadRequest
            ValidationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            FormatException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            InvalidTokenException => StatusCodes.Status400BadRequest,

            //404 NotFound
            NotFoundException => StatusCodes.Status404NotFound,
            KeyNotFoundException => StatusCodes.Status404NotFound,

            //403 Forbidden
            ForbiddenException => StatusCodes.Status403Forbidden,

            //401 Unauthorized
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

            //409 Conflict
            DbUpdateException => StatusCodes.Status409Conflict,
            MongoDuplicateKeyException => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status500InternalServerError
        };

    /// <summary>
    /// Resolves a short, stable <c>ProblemDetails</c> title based on the exception and the resolved HTTP status code.
    /// </summary>
    /// <remarks>
    /// This method centralizes the exception-to-status-code mapping for the API.
    /// </remarks>
    /// <param name="exception">
    /// The exception that triggered the error response. Cannot be null.
    /// </param>
    /// <param name="status">
    /// The HTTP status code resolved for <paramref name="exception"/>
    /// </param>
    /// <returns>
    /// A short title describing the error category.
    /// </returns>
    private static string ResolveTitle(Exception exception, int status) =>
        status switch
        {
            StatusCodes.Status400BadRequest when exception is
            InvalidTokenException => "Invalid token",
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Internal Server Error"
        };
}
