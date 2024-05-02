using Tests.Infrastructure;

namespace Tests.CollaboratorPayments;

public class ConfirmCollaboratorPaymentTests : BaseTest
{
    [Fact]
    public async Task confirm_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var proforma = await _appDsl.IssuedProforma(today);

        var (_, start) = await _appDsl.ProformaToCollaboratorPaymentProcess.Start(c =>
        {
            c.ProformaId = new[] { proforma.ProformaId };
        });

        await _appDsl.CollaboratorPayment.Pay(c =>
        {
            c.CollaboratorPaymentId = start!.CollaboratorPaymentId;
            c.PaidAt = today.AddDays(1);
        });

        await _appDsl.CollaboratorPayment.Upload("blank.pdf", c => c.CollaboratorPaymentId = start!.CollaboratorPaymentId);

        await _appDsl.CollaboratorPayment.Confirm(c =>
        {
            c.CollaboratorPaymentId = start!.CollaboratorPaymentId;
            c.ConfirmedAt = today.AddDays(1);
        });

    }
}
