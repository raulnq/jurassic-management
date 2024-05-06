namespace WebAPI.CollaboratorRoles;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollaboratorRoles(this IServiceCollection services)
    {
        services
            .AddTransient<ListCollaboratorRoles.Runner>()
            .AddTransient<GetCollaboratorRole.Runner>()
            .AddTransient<RegisterCollaboratorRole.Handler>()
            .AddTransient<EditCollaboratorRole.Handler>()
            .AddTransient<SearchCollaboratorRoles.Runner>();

        return services;
    }
}