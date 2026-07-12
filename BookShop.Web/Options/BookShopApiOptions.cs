namespace BookShop.Web.Options;

public sealed class BookShopApiOptions
{
    public const string SectionName = "BookShopApi";

    public string BaseUrl { get; set; } = string.Empty;
}