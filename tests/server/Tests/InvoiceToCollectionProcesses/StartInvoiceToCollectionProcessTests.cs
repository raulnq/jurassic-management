using Tests.Infrastructure;
using WebAPI.Proformas;

namespace Tests.InvoiceToCollectionProcesses;

public class StartInvoiceToCollectionProcessTests : BaseTest
{
    [Fact]
    public async Task start_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult) = await _appDsl.IssuedProforma(today);

        var invoice = await _appDsl.IssuedInvoice(proformaResult.ProformaId, clientResult.ClientId, proformaCommand.Currency, today);

        await _appDsl.InvoiceToCollectionProcess.Start(c =>
        {
            c.InvoiceId = new[] { invoice.InvoiceId };
        });

        await _appDsl.Proformas.ShouldBe(proformaResult.ProformaId, ProformaStatus.Invoiced);
    }

    [Fact]
    public async Task start_should_throw_an_error_when_invoice_not_issue()
    {
        var (proformaResult, proformaCommand, clientResult) = await _appDsl.IssuedProforma(_appDsl.Clock.Now.DateTime);

        var (_, start) = await _appDsl.ProformaToInvoiceProcess.Start(c =>
        {
            c.Currency = proformaCommand.Currency;
            c.ClientId = clientResult.ClientId;
            c.ProformaId = new[] { proformaResult.ProformaId };
        });

        await _appDsl.InvoiceToCollectionProcess.Start(c =>
        {
            c.InvoiceId = new[] { start!.InvoiceId };
        }, errorDetail: "code: invoice-is-not-issued");
    }
}