namespace WebAPI.ProformaToInvoiceProcesses;

public static class Endpoints
{
    public static void RegisterProformaToInvoiceProcessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/proforma-to-invoice-processes")
        .WithTags("proforma-to-invoice-processes");

        group.MapPost("/", StartProformaToInvoiceProcess.Handle);

    }
}