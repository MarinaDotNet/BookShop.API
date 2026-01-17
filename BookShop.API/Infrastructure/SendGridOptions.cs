using BookShop.API.Services;

namespace BookShop.API.Infrastructure;

public class SendGridOptions
{
    public string ApiKey { get; set; } = null!;

    public string FromEmail { get; set; } = null!; 

    public string FromName { get; set; } = null!;

    public string TemplateId { get; set; } = null!;
}
