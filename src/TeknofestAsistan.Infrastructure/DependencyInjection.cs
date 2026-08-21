using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Infrastructure.AI;
using TeknofestAsistan.Infrastructure.Documents;
using TeknofestAsistan.Infrastructure.Email;
using TeknofestAsistan.Infrastructure.Persistence;
using TeknofestAsistan.Infrastructure.Repositories;
using TeknofestAsistan.Infrastructure.Security;

namespace TeknofestAsistan.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("'DefaultConnection' bağlantı dizesi bulunamadı.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            }));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        var ollamaBaseUrl = configuration[$"{OllamaOptions.SectionName}:BaseUrl"] ?? "http://localhost:11434";

        services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(client =>
        {
            client.BaseAddress = new Uri(ollamaBaseUrl);
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        services.AddHttpClient<IAnswerGenerationService, OllamaAnswerGenerationService>(client =>
        {
            client.BaseAddress = new Uri(ollamaBaseUrl);
            client.Timeout = TimeSpan.FromMinutes(3);
        });

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IDocumentTextExtractor, DocumentTextExtractor>();

        services.Configure<GoogleOptions>(configuration.GetSection(GoogleOptions.SectionName));
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

        services.Configure<RecaptchaOptions>(configuration.GetSection(RecaptchaOptions.SectionName));
        services.AddHttpClient<IRecaptchaValidator, GoogleRecaptchaValidator>(client =>
        {
            client.BaseAddress = new Uri("https://www.google.com");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.Configure<BrevoOptions>(configuration.GetSection(BrevoOptions.SectionName));
        services.AddHttpClient<IEmailSender, BrevoEmailSender>(client =>
        {
            client.BaseAddress = new Uri("https://api.brevo.com");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
