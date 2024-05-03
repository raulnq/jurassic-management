using Tests.Infrastructure;

namespace Tests.CollaboratorPayments;

public class UploadDocumentTests : BaseTest
{
    [Fact]
    public async Task upload_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult) = await _appDsl.IssuedProforma(today);

        var (_, start) = await _appDsl.ProformaToCollaboratorPaymentProcess.Start(c =>
        {
            c.ProformaId = new[] { proformaResult.ProformaId };
        });

        await _appDsl.CollaboratorPayment.Pay(c =>
        {
            c.CollaboratorPaymentId = start!.CollaboratorPaymentId;
            c.PaidAt = today.AddDays(1);
        });

        await _appDsl.CollaboratorPayment.Upload("blank.pdf", c => c.CollaboratorPaymentId = start!.CollaboratorPaymentId);
    }
}