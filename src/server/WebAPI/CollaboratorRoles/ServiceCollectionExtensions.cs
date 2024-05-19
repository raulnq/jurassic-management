namespace WebAPI.CollaboratorRoles;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollaboratorRoles(this IServiceCollection services)
    {
        services
            .AddTransient<SearchCollaboratorRoles.Runner>();

        return services;
    }
}