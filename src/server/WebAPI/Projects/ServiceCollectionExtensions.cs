namespace WebAPI.Projects;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjects(this IServiceCollection services)
    {
        services
            .AddTransient<ListProjects.Runner>();

        return services;
    }
}