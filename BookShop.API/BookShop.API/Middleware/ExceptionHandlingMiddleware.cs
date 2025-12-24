using BookShop.API.Exceptions;
using MongoDB.Bson.IO;
using System.Net;

namespace BookShop.API.Middleware;

/// <summary>
/// Middleware that provides centralized exception handling for HTTP requests in the ASP.NET Core pipeline.
/// </summary>
/// <remarks>This middleware intercepts exceptions thrown during request processing and returns appropriate HTTP
/// status codes and error messages for known exception types. Unhandled exceptions are logged and result in a generic
/// 500 Internal Server Error response. Place this middleware early in the pipeline to ensure consistent error handling
/// across the application.</remarks>
/// <param name="next">The next middleware delegate in the request processing pipeline. Cannot be null.</param>
/// <param name="logger">The logger used to record exception details and diagnostic information. Cannot be null.</param>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    /// <summary>
    /// Processes an HTTP request and handles specific exceptions by returning appropriate HTTP status codes and error
    /// responses.
    /// </summary>
    /// <remarks>If a NotFoundException, ValidationException, ConflictException, or ForbidenException is
    /// thrown during request processing, this method sets the corresponding HTTP status code and returns a JSON error
    /// response. Any other unhandled exceptions result in a 500 Internal Server Error with a generic error message.
    /// This middleware should be registered early in the pipeline to ensure consistent error handling.</remarks>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the asynchronous operation of processing the HTTP request.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "NotFoundException caught.");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch(ValidationException ex)
        {
            _logger.LogWarning(ex, "ValidationException caught.");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch(ConflictException ex)
        {
            _logger.LogWarning(ex, "ConflictException caught.");
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch(ForbiddenException ex)
        {
            _logger.LogWarning(ex, "ForbidenException caught.");
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught.");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occured." });
        }

    }

}
