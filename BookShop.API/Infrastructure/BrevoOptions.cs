namespace BookShop.API.Infrastructure;

public class BrevoOptions
{
    public string ApiKey { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
    public string BaseAddress { get; set; } = null!;
}
