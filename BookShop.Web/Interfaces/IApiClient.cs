namespace BookShop.Web.Interfaces;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);
}