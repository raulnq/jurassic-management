namespace WebAPI.Projects;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjects(this IServiceCollection services)
    {
        services.AddTransient<ListProjects.Runner>();

        services.AddTransient<GetProject.Runner>();

        services.AddTransient<AddProject.Handler>();

        services.AddTransient<EditProject.Handler>();

        return services;
    }
}