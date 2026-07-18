namespace BookShop.Web.Interfaces;

/// <summary>
/// Provides methods for sending HTTP requests to the BookShop API.
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Sends an HTTP GET request and deserializes the JSON response.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the response body.
    /// </typeparam>
    /// <param name="requestUri">
    /// The relative request URI.
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
    Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);

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
    Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken = default);
}