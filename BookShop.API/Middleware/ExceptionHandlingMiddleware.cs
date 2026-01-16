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
    /// Middleware responsible for global exception handling.
    /// </summary>
    /// <remarks>
    /// This middleware intercepts unhandled exceptions thrown during request processing
    /// and converts them into standardized HTTP error responses using
    /// <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>.
    ///
    /// It ensures a single, consistent error-handling strategy across the application
    /// and prevents leaking internal exception details to API consumers.
    ///
    /// The middleware maps known application exceptions to appropriate HTTP status codes:
    /// <list type="bullet">
    /// <item>
    /// <description><see cref="ValidationException"/> → 400 (Bad Request)</description>
    /// </item>
    /// <item>
    /// <description><see cref="NotFoundException"/> → 404 (Not Found)</description>
    /// </item>
    /// <item>
    /// <description>Any other <see cref="ForbiddenException"/> → 403 (Forbidden)</description>
    /// </item>
    /// <item>
    /// <description>Any other <see cref="UnauthorizedAccessException"/> → 401 (Unauthorized)</description>
    /// </item>
    /// <item>
    /// <description><see cref="InvalidOperationException"/> → 409 (Conflict)</description>
    /// </item>
    /// <item>
    /// <description><see cref="DbUpdateException"/> → 409 (Conflict)</description>
    /// </item>
    /// <item>
    /// <description>Any other <see cref="Exception"/> → 500 (Internal Server Error)</description>
    /// </item>
    /// </list>
    ///
    /// All error responses follow the ProblemDetails format and include:
    /// <list type="bullet">
    /// <item>
    /// <description><c>status</c> — HTTP status code</description>
    /// </item>
    /// <item>
    /// <description><c>title</c> — error message</description>
    /// </item>
    /// <item>
    /// <description><c>detail</c> — inner error message (when applicable)</description>
    /// </item>
    /// <item>
    /// <description><c>instance</c> — request path</description>
    /// </item>
    /// </list>
    /// </remarks>
    private static Task HandleExceptionAsync(HttpContext context, Exception ex) 
    {
        var code = ex switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ForbiddenException => StatusCodes.Status403Forbidden,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status409Conflict,
            DbUpdateException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = code,
            Title = ex.Message,
            Detail = ex.InnerException?.Message,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = code;

        return context.Response.WriteAsJsonAsync(problem);
    }
}
