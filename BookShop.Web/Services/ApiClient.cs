using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using System.Net;
using System.Net.Http.Headers;

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
    /// Sends an HTTP GET request and deserializes the JSON response into the specified type. Automatically adds the current user's
    /// access token to the Authorization header when one is available in the authentication session.
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
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        await AddAuthorizationHeaderAsync(request);

        using HttpResponseMessage response = await SendAsync(request, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP POST request with a JSON payload and deserializes the JSON response. Automatically adds the current user's access token to the 
    /// Authorization header when one is available in the authentication session.
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

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(request)
        };

        await AddAuthorizationHeaderAsync(httpRequest);

        using HttpResponseMessage response = await SendAsync(httpRequest, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Response body was empty.");
    }

    /// <summary>
    /// Sends an HTTP POST request without expecting a response body. Automatically adds the current user's access token to the 
    /// Authorization header when one is available in the authentication session.
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

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(request)
        };

        await AddAuthorizationHeaderAsync(httpRequest);

        using HttpResponseMessage response = await SendAsync(httpRequest, cancellationToken);
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
    /// Attempts to wake the BookShop API by repeatdly calling the health endpoint.
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
        for(int i = 0; i < 3; i++)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("health", cancellationToken);

                if(response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch(HttpRequestException)
            { }
            catch(TaskCanceledException)
            { }
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }
    }

    /// <summary>
    /// Executes the specified HTTP operation and automatically retries it after attempting to wake the API if the 
    /// initial request fails.
    /// </summary>
    /// <typeparam name="TOperation">
    /// The type of the operation result.
    /// </typeparam>
    /// <param name="operation">
    /// The HTTP operation to execute.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The result returned by the specified operation.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when the operation fails after the retry attempt.
    /// </exception>
    /// <exception cref="TaskCanceledException">
    /// Thrown when the operation is canceled or times out after the retry attempt.
    private async Task<TOperation> ExecuteWithRetryAsync<TOperation>(Func<Task<TOperation>> operation, CancellationToken cancellationToken)
    {
        try
        {
            return await operation();
        }
        catch(HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            await EnsureApiIsAwakeAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            return await operation();
        }
        catch (HttpRequestException)
        {
            await EnsureApiIsAwakeAsync(cancellationToken);
            return await operation();
        }
        catch (TaskCanceledException)
        {
            await EnsureApiIsAwakeAsync(cancellationToken);
            return await operation();
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            HttpResponseMessage response = await 
                _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            response.EnsureSuccessStatusCode();
            return response;
        }, cancellationToken);
    }
}