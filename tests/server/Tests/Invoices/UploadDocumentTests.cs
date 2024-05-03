using Tests.Infrastructure;

namespace Tests.Invoices;

public class UploadDocumentTests : BaseTest
{
    [Fact]
    public async Task upload_should_be_ok()
    {
        var (proformaResult, proformaCommand, clientResult) = await _appDsl.IssuedProforma(_appDsl.Clock.Now.DateTime);

        var (_, start) = await _appDsl.ProformaToInvoiceProcess.Start(c =>
        {
            c.ClientId = clientResult.ClientId;
            c.Currency = proformaCommand.Currency;
            c.ProformaId = new[] { proformaResult.ProformaId };
        });

        await _appDsl.Invoice.Upload("blank.pdf", c => c.InvoiceId = start!.InvoiceId);
    }
}
