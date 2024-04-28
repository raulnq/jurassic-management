namespace WebAPI.Proformas;

public static class Endpoints
{
    public const string List = "/ui/proformas/list";

    public const string Register = "/ui/proformas/register";

    public const string View = "/ui/proformas/{proformaId}/view";

    public const string Issue = "/ui/proformas/{proformaId}/issue";

    public const string ListWeeks = "/ui/proformas/{proformaId}/weeks/list";

    public const string AddWorkItem = "/ui/proformas/{proformaId}/weeks/{week}/work-items/add";

    public const string EditWorkItem = "/ui/proformas/{proformaId}/weeks/{week}/work-items/{collaboratorId}/edit";

    public const string RemoveWorkItem = "/ui/proformas/{proformaId}/weeks/{week}/work-items/{collaboratorId}/remove";

    public const string ListWorkItems = "/ui/proformas/{proformaId}/weeks/{week}/work-items/list";

    public static void RegisterProformaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/proformas")
        .WithTags("proformas");

        group.MapGet("/", ListProformas.Handle);

        group.MapGet("/{proformaId:guid}/weeks", ListProformaWeeks.Handle);

        group.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items", ListProformaWeekWorkItems.Handle);

        group.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}", GetProformaWeekWorkItem.Handle);

        group.MapGet("/{proformaId:guid}/weeks/{week:int}", GetProformaWeek.Handle);

        group.MapGet("/{proformaId:guid}", GetProforma.Handle);

        group.MapPost("/", RegisterProforma.Handle);

        group.MapPost("/{proformaId:guid}/issue", IssueProforma.Handle);

        group.MapPost("/{proformaId:guid}/weeks/{week:int}/work-items", Proformas.AddWorkItem.Handle);

        group.MapPut("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}", Proformas.EditWorkItem.Handle);

        group.MapDelete("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}", Proformas.RemoveWorkItem.Handle);

        var uigroup = app.MapGroup("/ui/proformas")
        .ExcludeFromDescription();

        uigroup.MapGet("/list", ListProformas.HandlePage);

        uigroup.MapGet("/register", RegisterProforma.HandlePage);

        uigroup.MapPost("/register", RegisterProforma.HandleAction);

        uigroup.MapGet("/{proformaId:guid}/view", GetProforma.HandlePage);

        uigroup.MapGet("/{proformaId:guid}/issue", IssueProforma.HandlePage);

        uigroup.MapPost("/{proformaId:guid}/issue", IssueProforma.HandleAction);

        uigroup.MapGet("/{proformaId:guid}/weeks/list", ListProformaWeeks.HandlePage);

        uigroup.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items/list", ListProformaWeekWorkItems.HandlePage);

        uigroup.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items/add", Proformas.AddWorkItem.HandlePage);

        uigroup.MapPost("/{proformaId:guid}/weeks/{week:int}/work-items/add", Proformas.AddWorkItem.HandleAction);

        uigroup.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}/edit", Proformas.EditWorkItem.HandlePage);

        uigroup.MapPost("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}/edit", Proformas.EditWorkItem.HandleAction);

        uigroup.MapDelete("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}/remove", Proformas.RemoveWorkItem.HandleAction);
    }
}