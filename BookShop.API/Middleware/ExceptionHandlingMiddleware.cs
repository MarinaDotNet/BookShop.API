using BookShop.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson.IO;
using System.Net;

namespace BookShop.API.Middleware;

/// <summary>
/// Middleware that provides centralized exception handling for HTTP requests in the ASP.NET Core request pipeline.
/// </summary>
/// <remarks>
/// This middleware intercepts exceptions thrown during request processing and
/// converts them into appropriate HTTP error responses. Known exception types
/// are mapped to corresponding HTTP status codes, while unhandled exceptions
/// are logged and result in a generic 500 Internal Server Error response.
///
/// This middleware should be registered early in the pipeline to ensure
/// consistent error handling across the application.
/// </remarks>
/// <param name="next">
/// The next middleware delegate in the request pipeline.
/// </param>
/// <param name="logger">
/// The logger used to record exception details and diagnostic information.
/// </param>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    /// <summary>
    /// Processes the incoming HTTP request and handles exceptions thrown by
    /// downstream middleware or request handlers.
    /// </summary>
    /// <remarks>
    /// If a known exception is thrown during request processing, the exception
    /// is logged and an HTTP response with an appropriate status code is written.
    /// Unhandled exceptions result in a 500 Internal Server Error response.
    /// </remarks>
    /// <param name="context">
    /// The HTTP context for the current request.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }

    }

    /// <summary>
    /// Converts an unhandled exception into an RFC 7807 ProblemDetails response and writes it to the HTTP response.
    /// </summary>
    /// <param name="context">
    /// The current HTTP context.
    /// </param>
    /// <param name="exception">
    /// The exception to handle.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous write operation.
    /// </returns>
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception) 
    {
        var problem = ProblemDetailsBuilder.Build(context, exception);

        context.Response.StatusCode = problem.Status!.Value;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}
