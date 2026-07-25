namespace BookShop.Web.Models;

public sealed class ApiAwakeningViewModel
{
    

    public string Title { get; init; } = "BookShop API is currently sleeping.";

    public string Description { get; init; } = "This demo is hosted on Render's free plan. After a period of inactivity the API automatically goes to sleep and needs a short time to start again.";

    public ApiOperation Operation { get; init; } = ApiOperation.Generic;
}