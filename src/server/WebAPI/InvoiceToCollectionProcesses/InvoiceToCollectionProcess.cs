using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Invoices;
using WebAPI.Proformas;

namespace WebAPI.InvoiceToCollectionProcesses;

public class InvoiceToCollectionProcess
{
    public Guid CollectionId { get; private set; }
    public List<InvoiceToCollectionProcessItem> Items { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid ClientId { get; private set; }
    public Currency Currency { get; private set; }
    private InvoiceToCollectionProcess() { Items = []; }

    public InvoiceToCollectionProcess(Guid collectionId, Guid clientId, Currency currency, IEnumerable<Invoice> invoices, DateTimeOffset createdAt)
    {
        CollectionId = collectionId;
        ClientId = clientId;
        Currency = currency;
        CreatedAt = createdAt;
        Items = [];
        foreach (var invoice in invoices)
        {
            if (invoice.Status != InvoiceStatus.Issued)
            {
                throw new DomainException("invoice-is-not-issued");
            }
            Items.Add(new InvoiceToCollectionProcessItem(invoice.InvoiceId, CollectionId));
        }
    }
}
