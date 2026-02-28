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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddUserSecrets<StartupBase>();
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.Configure<BrevoOptions>(builder.Configuration.GetSection("Brevo"));
builder.Services.AddHttpClient<IAuthEmailSender, BrevoAuthEmailSender>((sp, http) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BrevoOptions>>().Value;
    if(string.IsNullOrWhiteSpace(options.BaseAddress))
        throw new InvalidOperationException("Brevo:BaseAddress is not configured.");
    if(string.IsNullOrWhiteSpace(options.ApiKey))
        throw new InvalidOperationException("Brevo:ApiKey is not configured.");

    http.BaseAddress = new Uri(options.BaseAddress);
    http.DefaultRequestHeaders.TryAddWithoutValidation("api-key", options.ApiKey);

    http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

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

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<AuthServices>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();


builder.Services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddSingleton<IRefreshTokenHasher, RefreshTokenHasher>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtOptions>() 
?? throw new InvalidOperationException("JWT settings are not properly configured. Please ensure that 'JwtSettings' section is set in the configuration.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(Options =>
{
    Options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret!))
    };
});
builder.Services.AddAuthorization();

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

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "BookShop API",
        Description = "An ASP.NET Core Web API for managing a book shop, including user authentication and book catalog operations.",
        Contact = new OpenApiContact
        {
            Name = "Marina",
            Email = "msichova@outlook.com",
            Url = new Uri("https://marinadotnet.github.io/")
        }
    });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {        
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: {your JWT token} to authenticate."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});


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
    app.UseSwaggerUI(c =>
    {
        c.EnablePersistAuthorization();
    });
}

if(!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
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
