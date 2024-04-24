﻿using Tests.Infrastructure;

namespace Tests.InvoiceToCollectionProcesses;

public class StartInvoiceToCollectionProcessTests : BaseTest
{
    [Fact]
    public async Task start_should_be_ok()
    {
        var proforma = await _appDsl.IssueProforma(_appDsl.Clock.Now.DateTime);

        var invoice = await _appDsl.IssueInvoice(proforma.ProformaId);

        await _appDsl.InvoiceToCollectionProcess.Start(c =>
        {
            c.InvoiceId = new[] { invoice.InvoiceId };
        });
    }

    [Fact]
    public async Task start_should_throw_an_error_when_invoice_not_issue()
    {
        var proforma = await _appDsl.IssueProforma(_appDsl.Clock.Now.DateTime);

        var (_, start) = await _appDsl.ProformaToInvoiceProcess.Start(c =>
        {
            c.ProformaId = new[] { proforma.ProformaId };
        });

        await _appDsl.InvoiceToCollectionProcess.Start(c =>
        {
            c.InvoiceId = new[] { start!.InvoiceId };
        }, errorDetail: "code: invoice-is-not-issued");
    }
}