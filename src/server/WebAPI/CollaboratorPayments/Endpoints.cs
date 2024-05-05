namespace WebAPI.CollaboratorPayments;

public static class Endpoints
{
    public const string List = "/ui/collaborator-payments/list";

    public const string View = "/ui/collaborator-payments/{collaboratorPaymentId}/view";

    public const string Pay = "/ui/collaborator-payments/{collaboratorPaymentId}/pay";

    public const string Confirm = "/ui/collaborator-payments/{collaboratorPaymentId}/confirm";

    public const string Upload = "/ui/collaborator-payments/{collaboratorPaymentId}/upload-document";

    public const string Cancel = "/ui/collaborator-payments/{collaboratorPaymentId}/cancel";

    public static string GetView(Guid collaboratorPaymentId)
    {
        return View.Replace("{collaboratorPaymentId}", collaboratorPaymentId.ToString());
    }
    public static string GetPay(Guid collaboratorPaymentId)
    {
        return Pay.Replace("{collaboratorPaymentId}", collaboratorPaymentId.ToString());
    }
    public static string GetConfirm(Guid collaboratorPaymentId)
    {
        return Confirm.Replace("{collaboratorPaymentId}", collaboratorPaymentId.ToString());
    }
    public static string GetCancel(Guid collaboratorPaymentId)
    {
        return Cancel.Replace("{collaboratorPaymentId}", collaboratorPaymentId.ToString());
    }

    public static string GetUpload(Guid collaboratorPaymentId)
    {
        return Upload.Replace("{collaboratorPaymentId}", collaboratorPaymentId.ToString());
    }
    public static void RegisterCollaboratorPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/collaborator-payments")
        .WithTags("collaborator-payments");

        group.MapPost("/{collaboratorPaymentId:guid}/upload-document", UploadDocument.Handle)
            .DisableAntiforgery();

        group.MapPost("/{collaboratorPaymentId:guid}/confirm", ConfirmCollaboratorPayment.Handle);

        group.MapPost("/{collaboratorPaymentId:guid}/pay", PayCollaboratorPayment.Handle);

        group.MapGet("/", ListCollaboratorPayments.Handle);

        var uigroup = app.MapGroup("/ui/collaborator-payments")
           .ExcludeFromDescription();

        uigroup.MapGet("/list", ListCollaboratorPayments.HandlePage);

        uigroup.MapGet("/{collaboratorPaymentId:guid}/view", GetCollaboratorPayment.HandlePage);

        uigroup.MapGet("/{collaboratorPaymentId:guid}/upload-document", UploadDocument.HandlePage);

        uigroup.MapPost("/{collaboratorPaymentId:guid}/upload-document", UploadDocument.HandleAction)
            .DisableAntiforgery();

        uigroup.MapGet("/{collaboratorPaymentId:guid}/pay", PayCollaboratorPayment.HandlePage);

        uigroup.MapPost("/{collaboratorPaymentId:guid}/pay", PayCollaboratorPayment.HandleAction);

        uigroup.MapGet("/{collaboratorPaymentId:guid}/confirm", ConfirmCollaboratorPayment.HandlePage);

        uigroup.MapPost("/{collaboratorPaymentId:guid}/confirm", ConfirmCollaboratorPayment.HandleAction);

        uigroup.MapPost("/{collaboratorPaymentId:guid}/cancel", CancelCollaboratorPayment.HandleAction);
    }
}
