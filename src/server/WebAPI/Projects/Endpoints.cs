namespace WebAPI.Projects;

public static class Endpoints
{
    public const string Search = "/ui/clients/search";

    public const string List = "/ui/clients/{clientId}/projects/list";

    public const string Register = "/ui/clients/{clientId}/projects/add";

    public const string Edit = "/ui/clients/{clientId}/projects/{projectId}/edit";

    public static void RegisterProjectEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/projects")
        .WithTags("projects");

        group.MapPost("/", AddProject.Handle);

        group.MapGet("/", ListProjects.Handle);

        group.MapGet("/{projectId:guid}", GetProject.Handle);

        group.MapPut("/{projectId:guid}", EditProject.Handle);

        var uigroup = app.MapGroup("/ui/clients")
        .ExcludeFromDescription();

        uigroup.MapGet("/{clientId:guid}/projects/list", ListProjects.HandlePage);

        uigroup.MapGet("/{clientId:guid}/projects/add", AddProject.HandlePage);

        uigroup.MapPost("/{clientId:guid}/projects/add", AddProject.HandleAction);

        uigroup.MapGet("/{clientId:guid}/projects/{projectId:guid}/edit", EditProject.HandlePage);

        uigroup.MapPost("/{clientId:guid}/projects/{projectId:guid}/edit", EditProject.HandleAction);

        uigroup.MapGet("/search", SearchProjects.HandlePage);
    }
}