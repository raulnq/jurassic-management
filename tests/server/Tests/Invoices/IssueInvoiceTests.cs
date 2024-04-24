using Tests.Infrastructure;

namespace Tests.Invoices;

public class IssueInvoiceTests : BaseTest
{
    [Fact]
    public async Task issue_should_be_ok()
    {
        var proforma = await _appDsl.IssueProforma(_appDsl.Clock.Now.DateTime);

        var (_, start) = await _appDsl.ProformaToInvoiceProcess.Start(c =>
        {
            c.ProformaId = new[] { proforma.ProformaId };
        });

        await _appDsl.Invoice.Upload("blank.pdf", c => c.InvoiceId = start!.InvoiceId);

        await _appDsl.Invoice.Issue(c => c.InvoiceId = start!.InvoiceId);

    }
}