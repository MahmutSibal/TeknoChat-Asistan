using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TeknofestAsistan.API.Hubs;
using TeknofestAsistan.API.Middleware;
using TeknofestAsistan.Application;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Domain.Entities;
using TeknofestAsistan.Domain.Enums;
using TeknofestAsistan.Infrastructure;
using TeknofestAsistan.Infrastructure.Persistence;
using TeknofestAsistan.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/teknofest-log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14));

// Add services to the container.

builder.Services.AddControllers(options =>
{
    // Every endpoint requires an authenticated caller by default; [AllowAnonymous] on
    // AuthController opts the login/register/forgot-password endpoints back out.
    options.Filters.Add(new AuthorizeFilter());
});
builder.Services.AddOpenApi();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Note: Swagger UI's "Authorize" padlock isn't wired up here (Microsoft.OpenApi 2.x reworked the
// security-scheme reference API). Test authenticated endpoints by adding an `Authorization: Bearer
// <token>` header manually (curl/Postman/Invoke-RestMethod) using the token from /api/auth/login.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();

builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

// Frontend origin(s) come from config so they can differ per environment without a code change —
// see appsettings.json "Cors:AllowedOrigins". Falls back to common local dev ports if unset.
var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    is { Length: > 0 } configuredOrigins
        ? configuredOrigins
        : ["http://localhost:3000", "http://localhost:5173", "http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(corsAllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            // SignalR's browser client negotiates with credentials:'include' even though we
            // authenticate via bearer token, not cookies — the preflight fails without this.
            // Safe alongside WithOrigins (only disallowed when combined with AllowAnyOrigin).
            .AllowCredentials());
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
        };

        // SignalR's browser client can't attach an Authorization header to the WebSocket
        // handshake, so it sends the JWT as ?access_token=... instead — accept it there only.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// Auth endpoints (login/register/forgot-password) are the brute-force / enumeration surface —
// throttle by client IP so an attacker can't hammer them at will.
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            title = "Çok fazla istek gönderildi.",
            detail = "Lütfen bir süre bekleyip tekrar deneyin.",
            status = StatusCodes.Status429TooManyRequests
        }, cancellationToken);
    };

    options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

var app = builder.Build();

// Automatically apply pending EF Core migrations and create missing tables mapping to the database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    // Bootstrap: UsersController (which provisions internal-role accounts) requires
    // SistemYoneticisi itself, so seed one on first run if none exists yet.
    if (!dbContext.Users.Any(u => u.Role == UserRole.SistemYoneticisi))
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var seedEmail = builder.Configuration["SeedAdmin:Email"] ?? "admin@teknofest.local";
        var seedPassword = builder.Configuration["SeedAdmin:Password"] ?? "Admin123!";

        dbContext.Users.Add(new ApplicationUser
        {
            FullName = "Sistem Yöneticisi",
            Email = seedEmail,
            PasswordHash = passwordHasher.Hash(seedPassword),
            Role = UserRole.SistemYoneticisi
        });
        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging();
app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Baseline security response headers on every request — cheap, no external dependency, closes
// off clickjacking (X-Frame-Options), MIME-sniffing (X-Content-Type-Options), and over-sharing
// referrer data on cross-origin navigations (Referrer-Policy).
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    await next();
});

// Blocks known AI-crawler / headless-automation User-Agents before they reach any endpoint.
// A determined attacker can spoof this header, but it stops the common, honest case (crawlers
// and automation frameworks that identify themselves) without touching legitimate API clients.
var blockedUserAgentPattern = new System.Text.RegularExpressions.Regex(
    "GPTBot|ChatGPT-User|CCBot|anthropic-ai|ClaudeBot|Claude-Web|Google-Extended|Bytespider|" +
    "PerplexityBot|HeadlessChrome|Selenium|Playwright|Puppeteer|PhantomJS",
    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
app.Use(async (context, next) =>
{
    var userAgent = context.Request.Headers.UserAgent.ToString();
    if (!string.IsNullOrEmpty(userAgent) && blockedUserAgentPattern.IsMatch(userAgent))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("Automated access is not permitted.");
        return;
    }
    await next();
});

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");

app.Run();
