namespace WebAPI.Projects;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjects(this IServiceCollection services)
    {
        services
            .AddTransient<ListProjects.Runner>()
            .AddTransient<GetProject.Runner>()
            .AddTransient<AddProject.Handler>()
            .AddTransient<EditProject.Handler>()
            .AddTransient<SearchProjects.Runner>();

        return services;
    }
}