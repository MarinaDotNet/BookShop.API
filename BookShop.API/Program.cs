using BookShop.API.Infrastructure;
using BookShop.API.Infrastructure.Persistence;
using BookShop.API.Mappings;
using BookShop.API.Middleware;
using BookShop.API.Models.Auth;
using BookShop.API.Repositories;
using BookShop.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddUserSecrets<StartupBase>();
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddScoped<IAuthEmailSender, SendGridAuthEmailSender>();
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGrid"));

builder.Services.AddDataProtection();
builder.Services.AddScoped<IAuthTokenService, AuthTokenService>();

builder.Services.Configure<AppUrlOptions>(builder.Configuration.GetSection("App"));
builder.Services.AddSingleton<IAuthLinkGenerator, AuthLinkGenerator>();

// Configure PostgreSQL DbContext
builder.Services.AddDbContext<AuthDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql")));

/// Dependency Injection
builder.Services.AddSingleton<IBookRepository, BookRepository>();
builder.Services.AddScoped<BookService>();

// Auto Mapper Configurations
builder.Services.AddAutoMapper(typeof(BookMapingProfile));

// Add ProblemDetails middleware
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Clear();
    app.Urls.Add($"http://*:{port}");
}

// Add Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => 
"Welcome to BookShop API! please visit: https://bookshop-api-xyxs.onrender.com/swagger/index.html for better user experience");

app.MapGet("/health", async (MongoDbContext context) =>
{
    try
    {
        var anyBook = await context.GetCollection().Find(_ => true).FirstOrDefaultAsync();
        return Results.Ok(new
        {
            status = "Healthy",
            database = "MongoDB",
            message = anyBook != null ? "Collection contains data" : "Collection is empty"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(new
        {
            status = "Unhealthy",
            database = "MongoDB",
            error = ex.Message
        }.ToString());
    }
});

app.Run();
