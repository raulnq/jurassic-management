namespace WebAPI.JiraProfiles;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJiraProfiles(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddTransient<GetJiraProfileProject.Runner>()
            .AddTransient<ListJiraProfileAccounts.Runner>();

        var settings = configuration.GetSection("Tempo").Get<TempoService.Settings>();

        services.AddHttpClient<TempoService>(client =>
        {
            client.BaseAddress = new Uri(settings!.Uri);
            client.Timeout = settings.Timeout;
        });

        return services;
    }
}