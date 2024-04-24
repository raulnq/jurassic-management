namespace WebAPI.Collaborators;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollaborators(this IServiceCollection services)
    {
        services.AddTransient<ListCollaborators.Runner>();

        services.AddTransient<GetCollaborator.Runner>();

        services.AddTransient<RegisterCollaborator.Handler>();

        services.AddTransient<EditCollaborator.Handler>();

        return services;
    }
}