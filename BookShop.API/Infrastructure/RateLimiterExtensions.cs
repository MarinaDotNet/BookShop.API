using System.Threading.RateLimiting;
using BookShop.API.Middleware;

namespace BookShop.API.Infrastructure;

/// <summary>
/// Provides extension methods for configuring application-wide rate limiting policies.
/// </summary>
public static class RateLimiterExtensions
{
    /// <summary>
    /// Default policy for general API requests.
    /// </summary>
    public const string Default = "DefaultPolicy";
    /// <summary>
    /// Policy applied to login requests.
    /// </summary>
    public const string Login = "LoginPolicy";
    /// <summary>
    /// Policy applied to password reset requests.
    /// </summary>
    public const string PasswordReset = "PasswordResetPolicy";
    /// <summary>
    /// Policy applied to user registration requests.
    /// </summary>
    public const string Register = "RegisterPolicy";
    /// <summary>
    /// Policy applied to email confirmation resend requests. 
    /// </summary>
    public const string ResendEmailConfirmation = "ResendEmailConfirmationPolicy";
    /// <summary>
    /// Policy applied to catalog browsing endpoints.
    /// </summary>
    public const string Catalog = "CatalogPolicy";

    /// <summary>
    /// Registers and configures all rate limiting policies used by the application.
    /// </summary>
    /// <param name="services">
    /// The service collection to configure.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance for chaining. 
    /// </returns>
    public static IServiceCollection AddApplicationRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";

                var problemDetails = ProblemDetailsBuilder.Build(
                    context.HttpContext, 
                    StatusCodes.Status429TooManyRequests, 
                    "Too many requests.", 
                    "Rate limit exceeded. Please try again latter.");

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
            };

            //Returns the client IP address used as the partition key for rate limiting.
            static string GetClientIpKey(HttpContext context) => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            options.AddPolicy(Default, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: GetClientIpKey(context),
                    factory: _ => CreateSlidingWindowOptions(100, TimeSpan.FromMinutes(1))));

            options.AddPolicy(Login, context => 
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: GetClientIpKey(context),
                    factory: _ => CreateSlidingWindowOptions(5, TimeSpan.FromMinutes(1))));
            
            options.AddPolicy(PasswordReset, context => 
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: GetClientIpKey(context),
                    factory: _ => CreateSlidingWindowOptions(3, TimeSpan.FromMinutes(10))));

            options.AddPolicy(Register, context => 
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: GetClientIpKey(context),
                    factory: _ => CreateSlidingWindowOptions(5, TimeSpan.FromHours(1))));
            
            options.AddPolicy(ResendEmailConfirmation, context => 
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: GetClientIpKey(context),
                    factory: _ => CreateSlidingWindowOptions(3, TimeSpan.FromMinutes(10))));

            options.AddPolicy(Catalog, context => 
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: GetClientIpKey(context),
                    factory: _ => CreateSlidingWindowOptions(200, TimeSpan.FromMinutes(1))));
        });
        return services;
    }

    /// <summary>
    /// Creates a sliding window rate limiter configuration.
    /// </summary>
    /// <param name="permitLimit">
    /// The maximum number of requests permitted within the specified time window.
    /// </param>
    /// <param name="window">
    /// The duration of the sliding window.
    /// </param>
    /// <returns>
    /// A configured <see cref="SlidingWindowRateLimiterOptions"/> instance.
    /// </returns>
    private static SlidingWindowRateLimiterOptions CreateSlidingWindowOptions(int permitLimit, TimeSpan window)
    {
        return new SlidingWindowRateLimiterOptions
        {
            PermitLimit= permitLimit,
            Window = window,
            SegmentsPerWindow = 2,
            QueueLimit = 0
        };
    }
}