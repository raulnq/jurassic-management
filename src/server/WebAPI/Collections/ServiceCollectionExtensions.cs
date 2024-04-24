namespace WebAPI.Collections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollections(this IServiceCollection services)
    {
        services.AddTransient<RegisterCollection.Handler>();

        services.AddTransient<ConfirmCollection.Handler>();

        services.AddTransient<ListCollections.Runner>();

        services.AddTransient<GetCollection.Runner>();

        return services;
    }
}