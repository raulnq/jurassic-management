namespace WebAPI.Clients;

public class Client
{
    public Guid ClientId { get; private set; }
    public string? Name { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? DocumentNumber { get; private set; }
    public string? Address { get; private set; }
    public decimal PenaltyMinimumHours { get; private set; }
    public decimal PenaltyAmount { get; private set; }
    public decimal TaxesExpensesPercentage { get; private set; }
    public decimal AdministrativeExpensesPercentage { get; private set; }
    public decimal BankingExpensesPercentage { get; private set; }
    public decimal MinimumBankingExpenses { get; private set; }

    private Client() { }

    public Client(Guid clientId,
        string name,
        string phoneNumber,
        string documentNumber,
        string address,
        decimal penaltyMinimumHours,
        decimal penaltyAmount,
        decimal taxesExpensesPercentage,
        decimal administrativeExpensesPercentage,
        decimal bankingExpensesPercentage,
        decimal minimumBankingExpenses)
    {
        ClientId = clientId;
        Name = name;
        PhoneNumber = phoneNumber;
        DocumentNumber = documentNumber;
        Address = address;
        PenaltyMinimumHours = penaltyMinimumHours;
        PenaltyAmount = penaltyAmount;
        TaxesExpensesPercentage = taxesExpensesPercentage;
        AdministrativeExpensesPercentage = administrativeExpensesPercentage;
        BankingExpensesPercentage = bankingExpensesPercentage;
        MinimumBankingExpenses = minimumBankingExpenses;
    }

    public void Edit(string name, string phoneNumber, string documentNumber, string address)
    {
        Name = name;
        PhoneNumber = phoneNumber;
        DocumentNumber = documentNumber;
        Address = address;
    }

    public void EditExpenses(decimal taxesExpensesPercentage,
        decimal administrativeExpensesPercentage,
        decimal bankingExpensesPercentage,
        decimal minimumBankingExpenses)
    {
        TaxesExpensesPercentage = taxesExpensesPercentage;
        AdministrativeExpensesPercentage = administrativeExpensesPercentage;
        BankingExpensesPercentage = bankingExpensesPercentage;
        MinimumBankingExpenses = minimumBankingExpenses;
    }

    public void EditPenalty(decimal penaltyMinimumHours, decimal penaltyAmount)
    {
        PenaltyMinimumHours = penaltyMinimumHours;
        PenaltyAmount = penaltyAmount;
    }
}
