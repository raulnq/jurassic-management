using Tests.Infrastructure;

namespace Tests.Invoices;

public class IssueInvoiceTests : BaseTest
{
    [Fact]
    public async Task issue_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult) = await _appDsl.IssuedProforma(today);

        var (_, start) = await _appDsl.ProformaToInvoiceProcess.Start(c =>
        {
            c.Currency = proformaCommand.Currency;
            c.ClientId = clientResult.ClientId;
            c.ProformaId = new[] { proformaResult.ProformaId };
        });

        await _appDsl.Invoice.Upload("blank.pdf", c => c.InvoiceId = start!.InvoiceId);

        await _appDsl.Invoice.Issue(c =>
        {
            c.InvoiceId = start!.InvoiceId;
            c.IssueAt = today.AddDays(1);
        });

    }
}

public class CancelInvoiceTests : BaseTest
{
    [Fact]
    public async Task cancel_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult) = await _appDsl.IssuedProforma(today);

        var (_, start) = await _appDsl.ProformaToInvoiceProcess.Start(c =>
        {
            c.Currency = proformaCommand.Currency;
            c.ClientId = clientResult.ClientId;
            c.ProformaId = new[] { proformaResult.ProformaId };
        });

        await _appDsl.Invoice.Cancel(c =>
        {
            c.InvoiceId = start!.InvoiceId;
        });

    }
}