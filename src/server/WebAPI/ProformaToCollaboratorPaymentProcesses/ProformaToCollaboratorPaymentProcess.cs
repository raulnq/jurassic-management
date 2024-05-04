using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;

namespace WebAPI.ProformaToCollaboratorPaymentProcesses;

public class ProformaToCollaboratorPaymentProcess
{
    public Guid CollaboratorPaymentId { get; private set; }
    public Guid CollaboratorId { get; private set; }
    public List<ProformaToCollaboratorPaymentProcessItem> Items { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Currency Currency { get; private set; }
    private ProformaToCollaboratorPaymentProcess() { Items = []; }

    public ProformaToCollaboratorPaymentProcess(Guid collaboratorPaymentId, Guid collaboratorId, Currency currency, IEnumerable<(Guid ProformaId, int Week, Guid CollaboratorId, ProformaStatus Status)> workItems, DateTimeOffset createdAt)
    {
        CollaboratorPaymentId = collaboratorPaymentId;
        CreatedAt = createdAt;
        CollaboratorId = collaboratorId;
        Items = [];
        Currency = currency;
        foreach (var workItem in workItems)
        {
            if (workItem.Status != ProformaStatus.Issued)
            {
                throw new DomainException("proforma-is-issued");
            }

            Items.Add(new ProformaToCollaboratorPaymentProcessItem(CollaboratorPaymentId, workItem.ProformaId, workItem.Week, workItem.CollaboratorId));
        }
    }
}
