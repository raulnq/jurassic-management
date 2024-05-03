using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;

namespace WebAPI.ProformaToInvoiceProcesses;

public class ProformaToInvoiceProcess
{
    public Guid InvoiceId { get; private set; }
    public Guid ClientId { get; private set; }
    public Currency Currency { get; private set; }
    public List<ProformaToInvoiceProcessItem> Items { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private ProformaToInvoiceProcess() { Items = []; }

    public ProformaToInvoiceProcess(Guid invoiceId, Guid clientId, Currency currency, IEnumerable<(Guid ProformaId, ProformaStatus Status)> proformas, DateTimeOffset createdAt)
    {
        InvoiceId = invoiceId;
        CreatedAt = createdAt;
        ClientId = clientId;
        Currency = currency;
        Items = [];
        foreach (var proforma in proformas)
        {
            if (proforma.Status != ProformaStatus.Issued)
            {
                throw new DomainException("proforma-is-not-issued");
            }
            Items.Add(new ProformaToInvoiceProcessItem(InvoiceId, proforma.ProformaId));
        }
    }
}
