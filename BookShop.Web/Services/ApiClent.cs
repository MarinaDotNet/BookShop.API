using BookShop.Web.Interfaces;

namespace BookShop.Web.Services;

public sealed class ApiClient(HttpClient httpClient) : IApiClient
{
    private readonly HttpClient _httpClent = httpClient;
}