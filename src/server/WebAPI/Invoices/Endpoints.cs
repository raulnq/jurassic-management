namespace WebAPI.Invoices;

public static class Endpoints
{
    public const string List = "/ui/invoices/list";

    public const string View = "/ui/invoices/{invoiceId}/view";

    public const string Issue = "/ui/invoices/{invoiceId}/issue";

    public const string Upload = "/ui/invoices/{invoiceId}/upload-document";

    public const string Cancel = "/ui/invoices/{invoiceId}/cancel";

    public static string GetView(Guid invoiceId)
    {
        return View.Replace("{invoiceId}", invoiceId.ToString());
    }
    public static string GetIssue(Guid invoiceId)
    {
        return Issue.Replace("{invoiceId}", invoiceId.ToString());
    }

    public static string GetCancel(Guid invoiceId)
    {
        return Cancel.Replace("{invoiceId}", invoiceId.ToString());
    }

    public static string GetUpload(Guid invoiceId)
    {
        return Upload.Replace("{invoiceId}", invoiceId.ToString());
    }

    public static void RegisterInvoiceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/invoices")
        .WithTags("invoices");

        group.MapPost("/{invoiceId:guid}/upload-document", UploadDocument.Handle)
            .DisableAntiforgery();

        group.MapPost("/{invoiceId:guid}/issue", IssueInvoice.Handle);

        group.MapPost("/{invoiceId:guid}/cancel", CancelInvoice.Handle);

        group.MapGet("/", ListInvoices.Handle);

        group.MapGet("/{invoiceId:guid}", GetInvoice.Handle);

        var uigroup = app.MapGroup("/ui/invoices")
           .ExcludeFromDescription();

        uigroup.MapGet("/list", ListInvoices.HandlePage);

        uigroup.MapGet("/{invoiceId:guid}/view", GetInvoice.HandlePage);

        uigroup.MapGet("/{invoiceId:guid}/upload-document", UploadDocument.HandlePage);

        uigroup.MapPost("/{invoiceId:guid}/upload-document", UploadDocument.HandleAction)
            .DisableAntiforgery();

        uigroup.MapGet("/{invoiceId:guid}/issue", IssueInvoice.HandlePage);

        uigroup.MapPost("/{invoiceId:guid}/issue", IssueInvoice.HandleAction);

        uigroup.MapPost("/{invoiceId:guid}/cancel", CancelInvoice.HandleAction);
    }
}
