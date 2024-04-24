using WebAPI.CollaboratorRoles;

namespace WebAPI.Proformas;

public class ProformaWeekWorkItem
{
    public Guid ProformaId { get; private set; }
    public int Week { get; private set; }
    public Guid CollaboratorId { get; private set; }
    public Guid CollaboratorRoleId { get; private set; }
    public decimal Hours { get; private set; }
    public decimal FreeHours { get; private set; }
    public decimal FeeAmount { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal ProfitAmount { get; private set; }
    public decimal ProfitPercentage { get; private set; }

    private ProformaWeekWorkItem()
    {

    }
    public ProformaWeekWorkItem(Guid proformId, int week, decimal hours, decimal freeHours, Guid collaboratorId, CollaboratorRole collaboratorRole)
    {
        ProformaId = proformId;
        Week = week;
        CollaboratorId = collaboratorId;
        CollaboratorRoleId = collaboratorRole.CollaboratorRoleId;
        Hours = hours;
        FreeHours = freeHours;
        FeeAmount = collaboratorRole.FeeAmount;
        ProfitPercentage = collaboratorRole.ProfitPercentage;
        Refresh();
    }

    private void Refresh()
    {
        SubTotal = (Hours - FreeHours) * FeeAmount;
        ProfitAmount = (SubTotal * ProfitPercentage) / 100;
    }

    public void Edit(decimal hours, decimal freeHours)
    {
        Hours = hours;
        FreeHours = freeHours;
        Refresh();
    }
}