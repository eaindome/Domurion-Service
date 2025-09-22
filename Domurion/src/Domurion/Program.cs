using Domurion.Services;
using Domurion.Data;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Domurion.Services.Interfaces;
using System.Threading.RateLimiting;
using System.Text;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

// Add CORS policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins("https://domurion-service.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
    options.AddPolicy("localhostCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PreferencesService>();
builder.Services.AddScoped<IPreferencesService, PreferencesService>();
builder.Services.AddScoped<PasswordVaultService>();
builder.Services.AddScoped<IPasswordVaultService, PasswordVaultService>();
builder.Services.AddScoped<Domurion.Helpers.EmailService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// Configure PostgreSQL with Entity Framework Core
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("fixed", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Custom policy for 2FA verification endpoints
    options.AddPolicy("2fa-verify", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(5),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured. Please set 'Jwt:Key' in your configuration.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId is not configured.");
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret is not configured.");
});


var app = builder.Build();

// Register global error handling middleware
app.UseMiddleware<Domurion.Helpers.ErrorHandlingMiddleware>();

// Custom middleware: extract JWT from cookie and set Authorization header
app.Use(async (context, next) =>
{
    const string cookieName = "access_token";
    if (context.Request.Cookies.ContainsKey(cookieName))
    {
        var token = context.Request.Cookies[cookieName];
        if (!string.IsNullOrEmpty(token))
        {
            // Only set if not already present
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Request.Headers.Authorization = $"Bearer {token}";
            }
        }
    }
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseCors("localhostCors"); // for local development
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("FrontendCors"); // for production
}

// Apply rate limiting globally; per-endpoint policies are set by endpoint metadata (e.g., [RateLimit] attribute in .NET 8+)

app.UseRateLimiter();
app.UseAuthentication();

// custom middleware for 401 JSON response
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 401 && !context.Response.HasStarted)
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new { error = "Unauthorized access." });
        await context.Response.WriteAsync(result);
    }
});
app.UseAuthorization();
app.MapControllers();
app.Run();

// For integration testing with WebApplicationFactory<Program>
public partial class Program
{
    public static void Main(string[] args)
    {
        // The actual logic is in the top-level statements above.
    }
}
