namespace WebAPI.Transactions;

public static class Endpoints
{
    public const string List = "/ui/transactions/list";

    public const string Register = "/ui/transactions/register";

    public const string Edit = "/ui/transactions/{transactionId}/edit";

    public const string Issue = "/ui/transactions/{transactionId}/issue";

    public const string Cancel = "/ui/transactions/{transactionId}/cancel";

    public const string Upload = "/ui/transactions/{transactionId}/upload-document";

    public static string GetEdit(Guid transactionId)
    {
        return Edit.Replace("{transactionId}", transactionId.ToString());
    }

    public static string GetIssue(Guid transactionId)
    {
        return Issue.Replace("{transactionId}", transactionId.ToString());
    }

    public static string GetCancel(Guid transactionId)
    {
        return Cancel.Replace("{transactionId}", transactionId.ToString());
    }

    public static string GetUpload(Guid transactionId)
    {
        return Upload.Replace("{transactionId}", transactionId.ToString());
    }

    public static void RegisterTransactionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/transactions")
        .WithTags("transactions");

        group.MapPost("/", RegisterTransaction.Handle);

        group.MapGet("/", ListTransactions.Handle);

        group.MapGet("/{transactionId:guid}", GetTransaction.Handle);

        group.MapPut("/{transactionId:guid}", EditTransaction.Handle);

        var uigroup = app.MapGroup("/ui/transactions")
        .ExcludeFromDescription();

        uigroup.MapGet("/list", ListTransactions.HandlePage);

        uigroup.MapGet("/register", RegisterTransaction.HandlePage);

        uigroup.MapPost("/register", RegisterTransaction.HandleAction);

        uigroup.MapGet("/{transactionId:guid}/edit", EditTransaction.HandlePage);

        uigroup.MapPost("/{transactionId:guid}/edit", EditTransaction.HandleAction);

        uigroup.MapGet("/{transactionId:guid}/upload-document", UploadDocument.HandlePage);

        uigroup.MapPost("/{transactionId:guid}/upload-document", UploadDocument.HandleAction)
            .DisableAntiforgery(); ;
    }
}
