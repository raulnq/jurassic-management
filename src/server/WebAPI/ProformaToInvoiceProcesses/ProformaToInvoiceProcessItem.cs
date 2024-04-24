namespace WebAPI.ProformaToInvoiceProcesses;

public class ProformaToInvoiceProcessItem
{
    public Guid InvoiceId { get; private set; }
    public Guid ProformaId { get; private set; }
    private ProformaToInvoiceProcessItem()
    {

    }

    public ProformaToInvoiceProcessItem(Guid invoiceId, Guid proformaId)
    {
        InvoiceId = invoiceId;
        ProformaId = proformaId;
    }
}
