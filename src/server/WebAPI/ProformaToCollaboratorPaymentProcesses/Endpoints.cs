namespace WebAPI.ProformaToCollaboratorPaymentProcesses;

public static class Endpoints
{
    public static void RegisterProformaToColaboratorPaymentProcessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/proforma-to-collaborator-payment-processes")
        .WithTags("proforma-to-collaborator-payment-processes");

        group.MapPost("/", StartProformaToCollaboratorPaymentProcess.Handle);

    }
}