namespace WebAPI.CollaboratorRoles;

public static class Endpoints
{
    public const string List = "/ui/collaborator-roles/list";

    public const string Register = "/ui/collaborator-roles/register";

    public const string Edit = "/ui/collaborator-roles/{collaboratorRoleId}/edit";

    public static string GetEdit(Guid collaboratorRoleId)
    {
        return Edit.Replace("{collaboratorRoleId}", collaboratorRoleId.ToString());
    }

    public static void RegisterCollaboratorRoleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/collaborator-roles")
        .WithTags("collaborator-roles")
        .RequireAuthorization();

        group.MapPost("/", RegisterCollaboratorRole.Handle);

        group.MapGet("/", ListCollaboratorRoles.Handle);

        group.MapGet("/{collaboratorRoleId:guid}", GetCollaboratorRole.Handle);

        group.MapPut("/{collaboratorRoleId:guid}", EditCollaboratorRole.Handle);

        var uigroup = app.MapGroup("/ui/collaborator-roles")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListCollaboratorRoles.HandlePage);

        uigroup.MapGet("/register", RegisterCollaboratorRole.HandlePage);

        uigroup.MapPost("/register", RegisterCollaboratorRole.HandleAction);

        uigroup.MapGet("/{collaboratorRoleId:guid}/edit", EditCollaboratorRole.HandlePage);

        uigroup.MapPost("/{collaboratorRoleId:guid}/edit", EditCollaboratorRole.HandleAction);
    }
}
