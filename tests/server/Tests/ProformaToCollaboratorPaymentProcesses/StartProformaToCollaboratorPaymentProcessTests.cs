using Tests.Infrastructure;

namespace Tests.ProformaToCollaboratorPaymentProcesses;

public class StartProformaToCollaboratorPaymentProcessTests : BaseTest
{
    [Fact]
    public async Task start_should_be_ok()
    {
        var (proformaResult, _, _) = await _appDsl.IssuedProforma(_appDsl.Clock.Now.DateTime);

        await _appDsl.ProformaToCollaboratorPaymentProcess.Start(c =>
        {
            c.ProformaId = new[] { proformaResult.ProformaId };
        });
    }

    [Fact]
    public async Task start_should_throw_an_error_when_proforma_not_issue()
    {
        var (proformaResult, _, _) = await _appDsl.RegisterProformaReadyToIssue(_appDsl.Clock.Now.DateTime);

        await _appDsl.ProformaToCollaboratorPaymentProcess.Start(c =>
        {
            c.ProformaId = new[] { proformaResult.ProformaId };
        }, errorDetail: "code: proforma-is-not-issued");
    }
}