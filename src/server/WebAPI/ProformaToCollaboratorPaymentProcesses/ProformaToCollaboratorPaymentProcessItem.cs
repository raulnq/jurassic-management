namespace WebAPI.ProformaToCollaboratorPaymentProcesses;

public class ProformaToCollaboratorPaymentProcessItem
{
    public Guid CollaboratorPaymentId { get; private set; }
    public Guid ProformaId { get; private set; }
    public int Week { get; private set; }
    public Guid CollaboratorId { get; private set; }
    private ProformaToCollaboratorPaymentProcessItem()
    {

    }

    public ProformaToCollaboratorPaymentProcessItem(Guid collaboratorPaymentId, Guid proformaId, int week, Guid collaboratorId)
    {
        CollaboratorPaymentId = collaboratorPaymentId;
        ProformaId = proformaId;
        Week = week;
        CollaboratorId = collaboratorId;
    }
}
