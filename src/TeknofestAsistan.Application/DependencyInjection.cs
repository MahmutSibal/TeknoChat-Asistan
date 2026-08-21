using Microsoft.Extensions.DependencyInjection;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Application.Services;

namespace TeknofestAsistan.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICompetitionService, CompetitionService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ISourceDocumentService, SourceDocumentService>();
        services.AddScoped<IChatQueryService, ChatQueryService>();
        services.AddScoped<ISupportTicketService, SupportTicketService>();
        services.AddScoped<IFaqEntryService, FaqEntryService>();

        return services;
    }
}
