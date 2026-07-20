namespace BookShop.Web.Interfaces;

/// <summary>
/// Provides methods for sending HTTP requests to the BookShop API.
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Sends an HTTP GET request and deserializes the JSON response. Automatically adds the current user's access token to the 
    /// Authorization header when one is available in the authentication session.
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
    Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken = default);

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
    Task PostAsync<TRequest>(string requestUri, TRequest request, CancellationToken cancellationToken = default);
}