namespace WebAPI.InvoiceToCollectionProcesses;

public static class Endpoints
{
    public static void RegisterInvoiceToCollectionProcessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/invoice-to-collection-processes")
        .WithTags("invoice-to-collection-processes");

        group.MapPost("/", StartInvoiceToCollectionProcess.Handle);

    }
}