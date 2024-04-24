using Tests.Infrastructure;

namespace Tests.CollaboratorPayments;

public class PayCollaboratorPaymentTests : BaseTest
{
    [Fact]
    public async Task pay_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var proforma = await _appDsl.IssueProforma(_appDsl.Clock.Now.DateTime);

        var (_, start) = await _appDsl.ProformaToCollaboratorPaymentProcess.Start(c =>
        {
            c.ProformaId = new[] { proforma.ProformaId };
        });

        await _appDsl.CollaboratorPayment.Pay(c =>
        {
            c.CollaboratorPaymentId = start!.CollaboratorPaymentId;
            c.PaidAt = today.AddDays(1);
        });
    }
}
