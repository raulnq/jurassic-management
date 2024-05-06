namespace WebAPI.Proformas;

public static class Endpoints
{
    public const string Title = "Proformas";

    public const string WeekTitle = "Weeks";

    public const string WorkItemTitle = "Work Items";

    public const string List = "/ui/proformas/list";

    public const string ListTitle = "List proformas";

    public const string Register = "/ui/proformas/register";

    public const string RegisterTitle = "Register proformas";

    public const string View = "/ui/proformas/{proformaId}/view";

    public const string ViewTitle = "View proforma";

    public const string Issue = "/ui/proformas/{proformaId}/issue";

    public const string IssueTitle = "Issue";

    public const string Cancel = "/ui/proformas/{proformaId}/cancel";

    public const string CancelTitle = "Cancel";

    public const string ListWeeks = "/ui/proformas/{proformaId}/weeks/list";

    public const string AddWorkItem = "/ui/proformas/{proformaId}/weeks/{week}/work-items/add";

    public const string AddWorkItemTitle = "Add work item";

    public const string EditWorkItem = "/ui/proformas/{proformaId}/weeks/{week}/work-items/{collaboratorId}/edit";

    public const string EditWorkItemTitle = "Edit work item";

    public const string RemoveWorkItem = "/ui/proformas/{proformaId}/weeks/{week}/work-items/{collaboratorId}/remove";

    public const string ListWorkItems = "/ui/proformas/{proformaId}/weeks/{week}/work-items/list";

    public const string SearchNotAddedToInvoice = "/ui/proformas/search-not-added-to-invoice";

    public const string SearchNotAddedToCollaboratorPayment = "/ui/proformas/search-not-added-to-collaborator-payment";

    public static string GetView(Guid proformaId)
    {
        return View.Replace("{proformaId}", proformaId.ToString());
    }

    public static string GetIssue(Guid proformaId)
    {
        return Issue.Replace("{proformaId}", proformaId.ToString());
    }

    public static string GetCancel(Guid proformaId)
    {
        return Cancel.Replace("{proformaId}", proformaId.ToString());
    }

    public static string GetListWeeks(Guid proformaId)
    {
        return ListWeeks.Replace("{proformaId}", proformaId.ToString());
    }

    public static string GetAddWorkItem(Guid proformaId, int week)
    {
        return AddWorkItem.Replace("{proformaId}", proformaId.ToString()).Replace("{week}", week.ToString());
    }

    public static string GetListWorkItems(Guid proformaId, int week)
    {
        return ListWorkItems.Replace("{proformaId}", proformaId.ToString()).Replace("{week}", week.ToString());
    }

    public static string GetEditWorkItem(Guid proformaId, int week, Guid collaboratorId)
    {
        return EditWorkItem.Replace("{proformaId}", proformaId.ToString())
            .Replace("{week}", week.ToString())
            .Replace("{collaboratorId}", collaboratorId.ToString());
    }

    public static string GetRemoveWorkItem(Guid proformaId, int week, Guid collaboratorId)
    {
        return RemoveWorkItem.Replace("{proformaId}", proformaId.ToString())
            .Replace("{week}", week.ToString())
            .Replace("{collaboratorId}", collaboratorId.ToString());
    }

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

        group.MapPost("/{proformaId:guid}/cancel", CancelProforma.Handle);

        group.MapPost("/{proformaId:guid}/weeks/{week:int}/work-items", Proformas.AddWorkItem.Handle);

        group.MapPut("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}", Proformas.EditWorkItem.Handle);

        group.MapDelete("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}", Proformas.RemoveWorkItem.Handle);

        var uigroup = app.MapGroup("/ui/proformas")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListProformas.HandlePage);

        uigroup.MapGet("/register", RegisterProforma.HandlePage);

        uigroup.MapPost("/register", RegisterProforma.HandleAction);

        uigroup.MapGet("/{proformaId:guid}/view", GetProforma.HandlePage);

        uigroup.MapGet("/{proformaId:guid}/issue", IssueProforma.HandlePage);

        uigroup.MapPost("/{proformaId:guid}/issue", IssueProforma.HandleAction);

        uigroup.MapPost("/{proformaId:guid}/cancel", CancelProforma.HandleAction);

        uigroup.MapGet("/{proformaId:guid}/weeks/list", ListProformaWeeks.HandlePage);

        uigroup.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items/list", ListProformaWeekWorkItems.HandlePage);

        uigroup.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items/add", Proformas.AddWorkItem.HandlePage);

        uigroup.MapPost("/{proformaId:guid}/weeks/{week:int}/work-items/add", Proformas.AddWorkItem.HandleAction);

        uigroup.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}/edit", Proformas.EditWorkItem.HandlePage);

        uigroup.MapPost("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}/edit", Proformas.EditWorkItem.HandleAction);

        uigroup.MapDelete("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}/remove", Proformas.RemoveWorkItem.HandleAction);

        uigroup.MapGet("/search-not-added-to-invoice", SearchProformasNotAddedToInvoice.HandlePage);

        uigroup.MapGet("/search-not-added-to-collaborator-payment", SearchProformasNotAddedToCollaboratorPayment.HandlePage);
    }
}