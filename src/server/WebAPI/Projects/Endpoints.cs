namespace WebAPI.Projects;

public static class Endpoints
{
    public const string Search = "/ui/clients/search";

    public const string List = "/ui/clients/{clientId}/projects/list";

    public const string Register = "/ui/clients/{clientId}/projects/add";

    public const string Edit = "/ui/clients/{clientId}/projects/{projectId}/edit";

    public static string GetEdit(Guid clientId, Guid projectId)
    {
        var temp = Edit.Replace("{clientId}", clientId.ToString());

        return temp.Replace("{projectId}", projectId.ToString());
    }

    public static string GetRegister(Guid clientId)
    {
        return Register.Replace("{clientId}", clientId.ToString());
    }

    public static string GetList(Guid clientId)
    {
        return List.Replace("{clientId}", clientId.ToString());
    }

    public static void RegisterProjectEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/clients")
        .WithTags("clients");

        group.MapPost("/{clientId:guid}/projects", AddProject.Handle);

        group.MapGet("/{clientId:guid}/projects", ListProjects.Handle);

        group.MapGet("/{clientId:guid}/projects/{projectId:guid}", GetProject.Handle);

        group.MapPut("/{clientId:guid}/projects/{projectId:guid}", EditProject.Handle);

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