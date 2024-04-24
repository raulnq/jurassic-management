namespace WebAPI.Proformas;

public static class Endpoints
{
    public static void RegisterProformaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/proformas")
        .WithTags("proformas");

        group.MapGet("/", ListProformas.Handle);

        group.MapGet("/{proformaId:guid}/weeks", ListProformaWeeks.Handle);

        group.MapGet("/{proformaId:guid}/weeks/{week:int}/collaborators", ListProformaWeekWorkItems.Handle);

        group.MapGet("/{proformaId:guid}/weeks/{week:int}/collaborators/{collaboratorId:guid}", GetProformaWeekWorkItem.Handle);

        group.MapGet("/{proformaId:guid}/weeks/{week:int}", GetProformaWeek.Handle);

        group.MapGet("/{proformaId:guid}", GetProforma.Handle);

        group.MapPost("/", RegisterProforma.Handle);

        group.MapPost("/{proformaId:guid}/issue", IssueProforma.Handle);

        group.MapPost("/{proformaId:guid}/weeks/{week:int}/collaborators/{collaboratorId:guid}", AddWorkItem.Handle);

        group.MapPut("/{proformaId:guid}/weeks/{week:int}/collaborators/{collaboratorId:guid}", EditWorkItem.Handle);

        group.MapDelete("/{proformaId:guid}/weeks/{week:int}/collaborators/{collaboratorId:guid}", RemoveWorkItem.Handle);

    }
}