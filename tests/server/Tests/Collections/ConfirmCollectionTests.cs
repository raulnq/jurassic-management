using Tests.Infrastructure;

namespace Tests.Collections;

public class ConfirmCollectionTests : BaseTest
{
    [Fact]
    public async Task confirm_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var proforma = await _appDsl.IssueProforma(today);

        var invoice = await _appDsl.IssueInvoice(proforma.ProformaId);

        var (_, start) = await _appDsl.InvoiceToCollectionProcess.Start(c =>
        {
            c.InvoiceId = new[] { invoice.InvoiceId };
        });

        var (_, collection) = await _appDsl.Collection.Get(q => q.CollectionId = start!.CollectionId);

        await _appDsl.Collection.Confirm(c =>
        {
            c.CollectionId = start!.CollectionId;
            c.ConfirmedAt = today.AddDays(1);
            c.Total = collection!.Total;
        });

    }
}