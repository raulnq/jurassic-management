namespace WebAPI.InvoiceToCollectionProcesses;

public class InvoiceToCollectionProcessItem
{
    public Guid InvoiceId { get; private set; }
    public Guid CollectionId { get; private set; }
    private InvoiceToCollectionProcessItem()
    {

    }

    public InvoiceToCollectionProcessItem(Guid invoiceId, Guid collectionId)
    {
        InvoiceId = invoiceId;
        CollectionId = collectionId;
    }
}