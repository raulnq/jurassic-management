namespace WebAPI.Clients;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClients(this IServiceCollection services)
    {
        services
            .AddTransient<ListClients.Runner>()
            .AddTransient<GetClient.Runner>()
            .AddTransient<RegisterClient.Handler>()
            .AddTransient<EditClient.Handler>()
            .AddTransient<SearchClients.Runner>();

        return services;
    }
}