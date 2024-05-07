namespace WebAPI.Balance;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBalance(this IServiceCollection services)
    {
        services
            .AddTransient<GetBalance.Runner>()
            .AddTransient<ListBalance.Runner>();

        return services;
    }
}