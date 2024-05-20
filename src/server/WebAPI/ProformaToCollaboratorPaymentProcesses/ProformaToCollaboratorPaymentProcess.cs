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

    public ProformaToCollaboratorPaymentProcess(Guid collaboratorPaymentId, Guid collaboratorId, Currency currency, IEnumerable<Proforma> proformas, DateTimeOffset createdAt)
    {
        CollaboratorPaymentId = collaboratorPaymentId;
        CreatedAt = createdAt;
        CollaboratorId = collaboratorId;
        Items = [];
        Currency = currency;
        foreach (var proforma in proformas)
        {
            if (proforma.Status != ProformaStatus.Issued)
            {
                throw new DomainException("proforma-is-issued");
            }

            foreach (var week in proforma.Weeks)
            {
                foreach (var item in week.WorkItems)
                {
                    Items.Add(new ProformaToCollaboratorPaymentProcessItem(CollaboratorPaymentId, item.ProformaId, item.Week, item.CollaboratorId));
                }
            }
        }
    }
}
