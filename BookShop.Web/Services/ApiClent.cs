using BookShop.Web.Interfaces;
using Microsoft.AspNetCore.Identity.Data;

namespace BookShop.Web.Services;

public sealed class ApiClient(HttpClient httpClient) : IApiClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code {(int)response.StatusCode}.");
        }

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP POST request with a JSON payload and deserializes the JSON response.
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

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code {(int)response.StatusCode}.");
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Response body was empty.");
    }
}