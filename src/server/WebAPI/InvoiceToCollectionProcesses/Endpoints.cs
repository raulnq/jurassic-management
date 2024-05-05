namespace WebAPI.InvoiceToCollectionProcesses;

public static class Endpoints
{
    public const string Register = "/ui/invoice-to-collection-processes/register";

    public const string ListItems = "/ui/invoice-to-collection-processes/{collectionId}/items/list";

    public static string GetListItems(Guid collectionId)
    {
        return ListItems.Replace("{collectionId}", collectionId.ToString());
    }

    public static void RegisterInvoiceToCollectionProcessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/invoice-to-collection-processes")
        .WithTags("invoice-to-collection-processes");

        group.MapPost("/", StartInvoiceToCollectionProcess.Handle);

        var uigroup = app.MapGroup("/ui/invoice-to-collection-processes")
        .ExcludeFromDescription();

        uigroup.MapGet("/register", StartInvoiceToCollectionProcess.HandlePage);

        uigroup.MapPost("/register", StartInvoiceToCollectionProcess.HandleAction);

        uigroup.MapGet("/{collectionId:guid}/items/list", ListInvoiceToCollectionProcessItems.HandlePage);
    }
}