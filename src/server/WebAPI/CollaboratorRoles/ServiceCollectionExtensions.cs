namespace WebAPI.CollaboratorRoles;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollaboratorRoles(this IServiceCollection services)
    {
        services.AddTransient<ListCollaboratorRoles.Runner>();

        services.AddTransient<GetCollaboratorRole.Runner>();

        services.AddTransient<RegisterCollaboratorRole.Handler>();

        services.AddTransient<EditCollaboratorRole.Handler>();

        services.AddTransient<SearchCollaboratorRoles.Runner>();

        return services;
    }
}