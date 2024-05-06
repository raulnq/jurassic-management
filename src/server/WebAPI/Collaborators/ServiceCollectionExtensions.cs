namespace WebAPI.Collaborators;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollaborators(this IServiceCollection services)
    {
        services
            .AddTransient<ListCollaborators.Runner>()
            .AddTransient<GetCollaborator.Runner>()
            .AddTransient<RegisterCollaborator.Handler>()
            .AddTransient<EditCollaborator.Handler>()
            .AddTransient<SearchCollaborators.Runner>();

        return services;
    }
}