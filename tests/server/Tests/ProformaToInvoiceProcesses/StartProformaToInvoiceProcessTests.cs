using Tests.Infrastructure;

namespace Tests.ProformaToInvoiceProcesses;

public class StartProformaToInvoiceProcessTests : BaseTest
{
    [Fact]
    public async Task start_should_be_ok()
    {
        var proforma = await _appDsl.IssuedProforma(_appDsl.Clock.Now.DateTime);

        await _appDsl.ProformaToInvoiceProcess.Start(c =>
        {
            c.ProformaId = new[] { proforma.ProformaId };
        });
    }

    [Fact]
    public async Task start_should_throw_an_error_when_proforma_not_issue()
    {
        var proforma = await _appDsl.RegisterProformaReadyToIssue(_appDsl.Clock.Now.DateTime);

        await _appDsl.ProformaToInvoiceProcess.Start(c =>
        {
            c.ProformaId = new[] { proforma.ProformaId };
        }, errorDetail: "code: proforma-is-not-issued");
    }
}