using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Net.Http.Headers;
using BookShop.Web.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BookShop.Web.Services;

/// <summary>
/// Provides a strongly typed client for communicating with the BookShop API. Handles JSON serialization, deserialization, 
/// authentication headers, and HTTP error handling for all outgoing requests.
/// </summary>
public sealed class ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : IApiClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Sends an HTTP GET request, deserialize the JSON response into the specified type, and relies on the underlying HTTP pipeline
    /// to recover from temporary API unavailability.
    /// </summary>
    /// <typeparam name="TResponse">
    /// The type of the response object to deserialize.
    /// </typeparam>
    /// <param name="requestUri">
    /// The relative URI of the API endpoint.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The deserialized response object.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when the API returns a non-success HTTP status code.
    /// </exception>
    public async Task<TResponse?> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage requestFactory() => new (HttpMethod.Get, requestUri);

        HttpResponseMessage response = await SendAsync(requestFactory, cancellationToken);
        try
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        }
        finally
        {
            response.Dispose();
        }
    }

    /// <summary>
    /// Sends an HTTP POST request with a JSON payload, automatically retries the request if neccessary, and deserializes the JSON 
    /// response. Authorization header is added automatically when an access token is available in the current authentication session.
    /// </summary>
    /// <typeparam name="TRequest">
    /// The type of the request body.
    /// </typeparam>
    /// <typeparam name="TResponse">
    /// The type of the response body.
    /// </typeparam>
    /// <param name="requestUri">
    /// The relative request URI.
    /// </param>
    /// <param name="request">
    /// The request payload.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The deserialized response.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when the server returns an unsuccessful status code.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the response body cannot be deserialized.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is null.
    /// </exception>
    public async Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        HttpRequestMessage requestFactory() =>
            new(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(request)
            };

        using HttpResponseMessage response = await SendAsync(requestFactory, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Response body was empty.");
    }

    /// <summary>
    /// Sends an HTTP POST request with a JSON payload, automatically retries the request if neccessary, and does not expect a response 
    /// body. Authorization header is added automatically when an access token is available in the current authentication session.
    /// </summary>
    /// <typeparam name="TRequest">
    /// The type of the request body to be serialized as JSON.
    /// </typeparam>
    /// <param name="requestUri">
    /// The relative URI of the API endpoint.
    /// </param>
    /// <param name="request">
    /// The request object to be serialized and sent in the request body. Cannot be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the HTTP request.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when the API returns a non-success HTTP status code.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="request"/> is <see langword="null"/>.
    /// </excepiton>
    public async Task PostAsync<TRequest>(string requestUri, TRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        HttpRequestMessage requestFactory() => 
            new(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(request)
            };

        using HttpResponseMessage response = await SendAsync(requestFactory, cancellationToken);
    }

    /// <summary>
    /// Adds the current user's access token to the Authorization header of the specified HTTP request, if an access token is available
    /// in the current authentication session.
    /// </summary>
    /// <param name="request">
    /// The HTTP request message to which the Authorization header will be added.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    private async Task AddAuthorizationHeaderAsync(HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var accessToken = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    /// <summary>
    /// Attempts to wake the BookShop API by repeatdly calling the health endpoint. This helps recover from cold starts on hosting
    /// platforms such as Render. This method performs a best-effort wake-up attempt and intentionally ignores transient failures.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <remarks>
    /// This method is intended to handle cold starts on hosting platforms such as Render. It retries the health 
    /// check several times before giving up.
    /// </remarks>
    private async Task EnsureApiIsAwakeAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var serviceClient = new HttpClient();
            serviceClient.Timeout = TimeSpan.FromMinutes(2.5);

            var baseAddress = _httpClient.BaseAddress?.ToString() ?? "https://bookshop-api-xyxs.onrender.com/";

            for(int i = 0; i < 3; i++)
            {
                try
                {
                    var response = await serviceClient.GetAsync(baseAddress, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }
                }
                catch{}

                await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
            }
        }
        catch{}
    }

    /// <summary>
    /// Sends an HTTP request, automatically adds the current user's authorization header, validates the HTTP respnse, and returns it.
    /// If the request fails, a best-effort API wake-up attempt is started before the exception is rethrown.
    /// </summary>
    /// <param name="requestFactory">
    /// A delegate that creates a new <see cref="HttpRequestMessage"/> instance for each execution attempt. 
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A successful HTTP response returned by the API.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when the request fails or the server returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="TaskCanceledException">
    /// Thrown when the operation is canceled.
    /// </exception>
    private async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        using HttpRequestMessage request = requestFactory();
        await AddAuthorizationHeaderAsync(request);

        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request, linkedCts.Token);

            if(response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                response.Dispose();
                throw new HttpRequestException("API is waking up.", null, HttpStatusCode.TooManyRequests);
            }

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            ApiProblemDetails? problem = null;

            try
            {
                problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(cancellationToken: cancellationToken);
            }
            catch(JsonException)
            {
                //The response body is not a valid ProblemDetails JSON. 
                // Fall back to a generic HTTP error message.
            }

            string message = problem?.Detail
                            ?? problem?.Title
                            ?? $"The server returned HTTP {(int)response.StatusCode} ({response.StatusCode}).";
            HttpStatusCode statusCode = response.StatusCode;

            response.Dispose();
            throw new HttpRequestException(message, null, statusCode);
            
        }
        catch(Exception ex) when (
            ex is OperationCanceledException ||
            ex is HttpRequestException )
        {
            _ = EnsureApiIsAwakeAsync(CancellationToken.None);
            
            throw;
        }
    }
}