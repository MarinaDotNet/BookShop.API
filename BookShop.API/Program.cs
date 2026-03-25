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
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

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

// Dependency Injection
builder.Services.AddSingleton<IBookRepository, BookRepository>();
builder.Services.AddScoped<BookService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<AuthServices>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();


builder.Services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddSingleton<IRefreshTokenHasher, RefreshTokenHasher>();

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(3, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtOptions>() 
?? throw new InvalidOperationException("JWT settings are not properly configured. Please ensure that 'JwtSettings' section is set in the configuration.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret!))
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier);
            var iatClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Iat);

            if(userIdClaim is null || iatClaim is null)
            {
                context.Fail("Invalid access token.");
                return;
            }

            if(!int.TryParse(userIdClaim.Value, out var userId))
            {
                context.Fail("Invalid user identifier.");
                return;
            }

            if(!long.TryParse(iatClaim.Value, out var issuedAtUnix))
            {
                context.Fail("Invalid token issue time.");
                return;
            }

            var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

            var user = await userRepository.GetUserByIdAsync(userId, context.HttpContext.RequestAborted);

            if(user is null || user.IsDeleted || !user.IsActive)
            {
                context.Fail("User is not available.");
                return;
            }

            var issuedAtUtc = DateTimeOffset.FromUnixTimeSeconds(issuedAtUnix).UtcDateTime;
            if(issuedAtUtc <= user.SecurityTokenInvalidBeforeUtc)
            {
                context.Fail("Access token is no longer valid.");
            }
        }
    };

});
builder.Services.AddAuthorization();

// Auto Mapper Configurations
builder.Services.AddAutoMapper(_ => {}, typeof(BookMapingProfile), typeof(UserMapingProfile));

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
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.AddCors(options =>
{
    var publicOrigins = builder.Configuration["Cors:WebAppOrigins"]
        ?? throw new InvalidOperationException("Cors:WebAppOrigins not configured");
    var adminOrigins = builder.Configuration["Cors:AdminPanelOrigins"]
        ?? throw new InvalidOperationException("Cors:AdminPanelOrigins not configured");

    options.AddPolicy("AdminPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(adminOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
        } 
    });
    
    options.AddPolicy("PublicPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyHeader()
              .WithMethods("GET")
              .AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(publicOrigins)
              .AllowAnyHeader()
              .WithMethods("GET");
        }
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
    app.UseSwaggerUI(options =>
    {
        options.EnablePersistAuthorization();

        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach(var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
}

if(!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

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
