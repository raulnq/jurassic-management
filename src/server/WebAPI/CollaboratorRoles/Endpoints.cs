namespace WebAPI.CollaboratorRoles;

public static class Endpoints
{
    public class Endpoint
    {
        public string List = "/ui/collaborator-roles/list";

        public string Register = "/ui/collaborator-roles/register";

        public string Edit = "/ui/collaborator-roles/{collaboratorRoleId}/edit";
    }

    public static readonly Endpoint Instance = new();

    public static void RegisterCollaboratorRoleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/collaborator-roles")
        .WithTags("collaborator-roles");

        group.MapPost("/", RegisterCollaboratorRole.Handle);

        group.MapGet("/", ListCollaboratorRoles.Handle);

        group.MapGet("/{collaboratorRoleId:guid}", GetCollaboratorRole.Handle);

        group.MapPut("/{collaboratorRoleId:guid}", EditCollaboratorRole.Handle);

        var uigroup = app.MapGroup("/ui/collaborator-roles")
        .ExcludeFromDescription();

        uigroup.MapGet("/list", ListCollaboratorRoles.HandlePage);

        uigroup.MapGet("/register", RegisterCollaboratorRole.HandlePage);

        uigroup.MapPost("/register", RegisterCollaboratorRole.HandleAction);

        uigroup.MapGet("/{collaboratorRoleId:guid}/edit", EditCollaboratorRole.HandlePage);

        uigroup.MapPost("/{collaboratorRoleId:guid}/edit", EditCollaboratorRole.HandleAction);
    }
}