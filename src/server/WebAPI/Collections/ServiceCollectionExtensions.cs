namespace WebAPI.Collections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollections(this IServiceCollection services)
    {
        services
            .AddTransient<RegisterCollection.Handler>()
            .AddTransient<ConfirmCollection.Handler>()
            .AddTransient<ListCollections.Runner>()
            .AddTransient<GetCollection.Runner>()
            .AddTransient<CancelCollection.Handler>();

        return services;
    }
}