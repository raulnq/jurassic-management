using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Invoices;

namespace WebAPI.InvoiceToCollectionProcesses;

public class InvoiceToCollectionProcess
{
    public Guid CollectionId { get; private set; }
    public List<InvoiceToCollectionProcessItem> Items { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private InvoiceToCollectionProcess() { Items = []; }

    public InvoiceToCollectionProcess(Guid collectionId, IEnumerable<(Guid InvoiceId, InvoiceStatus Status)> proformas, DateTimeOffset createdAt)
    {
        CollectionId = collectionId;
        CreatedAt = createdAt;
        Items = [];
        foreach (var proforma in proformas)
        {
            if (proforma.Status != InvoiceStatus.Issued)
            {
                throw new DomainException("invoice-is-not-issued");
            }
            Items.Add(new InvoiceToCollectionProcessItem(proforma.InvoiceId, CollectionId));
        }
    }
}
