namespace WebAPI.Collaborators;

public static class Endpoints
{
    public class Endpoint
    {
        public string List = "/ui/collaborators/list";

        public string Register = "/ui/collaborators/register";

        public string Edit = "/ui/collaborators/{collaboratorId}/edit";
    }

    public static readonly Endpoint Instance = new();

    public static void RegisterCollaboratorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/collaborators")
        .WithTags("collaborators");

        group.MapPost("/", RegisterCollaborator.Handle);

        group.MapGet("/{collaboratorId:guid}", GetCollaborator.Handle);

        group.MapPut("/{collaboratorId:guid}", EditCollaborator.Handle);

        group.MapGet("/", ListCollaborators.Handle);

        var uigroup = app.MapGroup("/ui/collaborators")
            .ExcludeFromDescription();

        uigroup.MapGet("/list", ListCollaborators.HandlePage);

        uigroup.MapGet("/register", RegisterCollaborator.HandlePage);

        uigroup.MapPost("/register", RegisterCollaborator.HandleAction);

        uigroup.MapGet("/{collaboratorId:guid}/edit", EditCollaborator.HandlePage);

        uigroup.MapPost("/{collaboratorId:guid}/edit", EditCollaborator.HandleAction);

    }
}