namespace WebAPI.Clients;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClients(this IServiceCollection services)
    {
        services.AddTransient<ListClients.Runner>();

        services.AddTransient<GetClient.Runner>();

        services.AddTransient<RegisterClient.Handler>();

        services.AddTransient<EditClient.Handler>();

        services.AddTransient<SearchClients.Runner>();

        return services;
    }
}