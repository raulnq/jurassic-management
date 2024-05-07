namespace WebAPI.CollaboratorBalance;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollaboratorBalance(this IServiceCollection services)
    {
        services
            .AddTransient<GetCollaboratorBalance.Runner>()
            .AddTransient<ListCollaboratorBalance.Runner>();

        return services;
    }
}