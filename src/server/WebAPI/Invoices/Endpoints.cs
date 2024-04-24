namespace WebAPI.Invoices;

public static class Endpoints
{
    public static void RegisterInvoiceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/invoices")
        .WithTags("invoices");

        group.MapPost("/{invoiceId:guid}/upload-document", UploadDocument.Handle)
            .DisableAntiforgery();

        group.MapPost("/{invoiceId:guid}/issue", IssueInvoice.Handle);

        group.MapGet("/", ListInvoices.Handle);

        group.MapGet("/{invoiceId:guid}", GetInvoice.Handle);
    }
}