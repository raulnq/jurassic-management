namespace WebAPI.Invoices;

public static class Endpoints
{
    public const string List = "/ui/invoices/list";

    public const string View = "/ui/invoices/{invoiceId}/view";

    public static string GetView(Guid invoiceId)
    {
        return View.Replace("{invoiceId}", invoiceId.ToString());
    }

    public static void RegisterInvoiceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/invoices")
        .WithTags("invoices");

        group.MapPost("/{invoiceId:guid}/upload-document", UploadDocument.Handle)
            .DisableAntiforgery();

        group.MapPost("/{invoiceId:guid}/issue", IssueInvoice.Handle);

        group.MapGet("/", ListInvoices.Handle);

        group.MapGet("/{invoiceId:guid}", GetInvoice.Handle);

        var uigroup = app.MapGroup("/ui/invoices")
           .ExcludeFromDescription();

        uigroup.MapGet("/list", ListInvoices.HandlePage);
    }
}
