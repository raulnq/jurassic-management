using Tests.Infrastructure;

namespace Tests.CollaboratorPayments;

public class PayCollaboratorPaymentTests : BaseTest
{
    [Fact]
    public async Task pay_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult, collaboratorResult) = await _appDsl.IssueProforma(_appDsl.Clock.Now.DateTime);

        await _appDsl.RegisterInvoice(proformaResult.ProformaId, clientResult.ClientId, proformaCommand.Currency);

        var start = await _appDsl.RegisterCollaboratorPayment(proformaResult.ProformaId, collaboratorResult.CollaboratorId, proformaCommand.Currency);

        await _appDsl.CollaboratorPayment.Pay(c =>
        {
            c.CollaboratorPaymentId = start!.CollaboratorPaymentId;
            c.PaidAt = today.AddDays(1);
        });
    }
}
