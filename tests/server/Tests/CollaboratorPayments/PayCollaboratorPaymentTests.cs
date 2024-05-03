using Tests.Infrastructure;

namespace Tests.CollaboratorPayments;

public class PayCollaboratorPaymentTests : BaseTest
{
    [Fact]
    public async Task pay_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult) = await _appDsl.IssuedProforma(_appDsl.Clock.Now.DateTime);

        var (_, start) = await _appDsl.ProformaToCollaboratorPaymentProcess.Start(c =>
        {
            c.ProformaId = new[] { proformaResult.ProformaId };
        });

        await _appDsl.CollaboratorPayment.Pay(c =>
        {
            c.CollaboratorPaymentId = start!.CollaboratorPaymentId;
            c.PaidAt = today.AddDays(1);
        });
    }
}
