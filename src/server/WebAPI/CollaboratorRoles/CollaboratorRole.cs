namespace WebAPI.CollaboratorRoles;

public class CollaboratorRole
{
    public Guid CollaboratorRoleId { get; private set; }
    public string? Name { get; private set; }
    public decimal FeeAmount { get; private set; }
    public decimal ProfitPercentage { get; private set; }

    private CollaboratorRole() { }

    public CollaboratorRole(Guid collaboratorRoleId, string name, decimal feeAmount, decimal profitPercentage)
    {
        CollaboratorRoleId = collaboratorRoleId;
        Name = name;
        FeeAmount = feeAmount;
        ProfitPercentage = profitPercentage;
    }

    public void Edit(string name, decimal feeAmount, decimal profitPercentage)
    {
        Name = name;
        FeeAmount = feeAmount;
        ProfitPercentage = profitPercentage;
    }
}
