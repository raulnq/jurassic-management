namespace WebAPI.CollaboratorPayments;

public static class Endpoints
{
    public static void RegisterCollaboratorPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/collaborator-payments")
        .WithTags("collaborator-payments");

        group.MapPost("/{collaboratorPaymentId:guid}/upload-document", UploadDocument.Handle)
            .DisableAntiforgery();

        group.MapPost("/{collaboratorPaymentId:guid}/confirm", ConfirmCollaboratorPayment.Handle);

        group.MapPost("/{collaboratorPaymentId:guid}/pay", PayCollaboratorPayment.Handle);

        //group.MapGet("/", ListInvoices.Handle);

        //group.MapGet("/{invoiceId:guid}", GetInvoice.Handle);
    }
}