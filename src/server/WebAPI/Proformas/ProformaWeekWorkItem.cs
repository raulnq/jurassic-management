using WebAPI.CollaboratorRoles;
using WebAPI.Collaborators;

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
    public decimal Withholding { get; private set; }
    public decimal WithholdingPercentage { get; private set; }
    public decimal GrossSalary { get; private set; }
    public decimal NetSalary { get; private set; }

    private ProformaWeekWorkItem()
    {

    }
    public ProformaWeekWorkItem(Guid proformId, int week, decimal hours, decimal freeHours, CollaboratorRole collaboratorRole, Collaborator collaborator)
    {
        ProformaId = proformId;
        Week = week;
        CollaboratorId = collaborator.CollaboratorId;
        CollaboratorRoleId = collaboratorRole.CollaboratorRoleId;
        Hours = hours;
        FreeHours = freeHours;
        FeeAmount = collaboratorRole.FeeAmount;
        ProfitPercentage = collaboratorRole.ProfitPercentage;
        WithholdingPercentage = collaborator.WithholdingPercentage;
        Refresh();
    }

    private void Refresh()
    {
        SubTotal = (Hours - FreeHours) * FeeAmount;
        ProfitAmount = (SubTotal * ProfitPercentage) / 100;
        GrossSalary = SubTotal - ProfitAmount;
        Withholding = (GrossSalary * WithholdingPercentage) / 100;
        NetSalary = GrossSalary - Withholding;
    }

    public void Edit(decimal hours, decimal freeHours)
    {
        Hours = hours;
        FreeHours = freeHours;
        Refresh();
    }
}