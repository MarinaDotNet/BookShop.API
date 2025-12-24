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
    /// Processes the incoming HTTP request and handles specific exceptions by returning appropriate HTTP status codes
    /// and error messages to the client.
    /// </summary>
    /// <remarks>If a known exception is thrown during request processing, the method logs the exception and
    /// writes an error response with a corresponding status code. Unhandled exceptions result in a 500 Internal Server
    /// Error response. This method should be used as middleware in the ASP.NET Core request pipeline.</remarks>
    /// <param name="context">The HTTP context for the current request. Provides access to request and response information.</param>
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
            await WriteErrorAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch(ValidationException ex)
        {
            _logger.LogWarning(ex, "ValidationException caught.");
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch(ConflictException ex)
        {
            _logger.LogWarning(ex, "ConflictException caught.");
            await WriteErrorAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch(ForbiddenException ex)
        {
            _logger.LogWarning(ex, "ForbidenException caught.");
            await WriteErrorAsync(context, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught.");
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }

    }

    /// <summary>
    /// Asynchronously writes a JSON-formatted error response to the specified HTTP context with the given status code
    /// and error message.
    /// </summary>
    /// <remarks>The response content type is set to "application/json". The response body will contain a
    /// single property named "error" with the provided message.</remarks>
    /// <param name="context">The HTTP context to which the error response will be written. Must not be null.</param>
    /// <param name="statusCode">The HTTP status code to set for the response. Common values include 400 for bad requests or 500 for server
    /// errors.</param>
    /// <param name="message">The error message to include in the JSON response body. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = message
        });
    }

}
