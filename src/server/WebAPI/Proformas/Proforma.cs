using WebAPI.Clients;
using WebAPI.CollaboratorRoles;
using WebAPI.Infrastructure.ExceptionHandling;

namespace WebAPI.Proformas;

public enum ProformaStatus
{
    Pending = 0,
    Issued = 1,
    Canceled = 2
}

public enum Currency
{
    PEN = 0,
    USD = 1
}

public class Proforma
{
    public Guid ProformaId { get; private set; }
    public Guid ProjectId { get; private set; }
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }
    public string Number { get; private set; }
    public List<ProformaWeek> Weeks { get; private set; }
    public decimal Total { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal Commission { get; private set; }
    public decimal Discount { get; private set; }
    public decimal MinimumHours { get; private set; }
    public decimal PenaltyMinimumHours { get; private set; }
    public decimal TaxesExpensesPercentage { get; private set; }
    public decimal AdministrativeExpensesPercentage { get; private set; }
    public decimal BankingExpensesPercentage { get; private set; }
    public decimal MinimumBankingExpenses { get; private set; }
    public decimal TaxesExpensesAmount { get; private set; }
    public decimal AdministrativeExpensesAmount { get; private set; }
    public decimal BankingExpensesAmount { get; private set; }
    public ProformaStatus Status { get; private set; }
    public DateTimeOffset? IssuedAt { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Currency Currency { get; private set; }

    private Proforma() { Weeks = []; }

    public Proforma(Guid proformaId, DateTime start, DateTime end, Guid projectId, Client client, DateTimeOffset createdAt, decimal discount, Currency currency, int count)
    {
        ProformaId = proformaId;
        CreatedAt = createdAt;
        Status = ProformaStatus.Pending;
        Start = start;
        End = end;
        Number = $"{End.ToString("yyyyMMdd")}-{count + 1}";
        ProjectId = projectId;
        Currency = currency;
        Weeks = [];
        EnsureValidWeeklyPeriod();
        MinimumHours = client.PenaltyMinimumHours;
        PenaltyMinimumHours = client.PenaltyAmount;
        TaxesExpensesPercentage = client.TaxesExpensesPercentage;
        AdministrativeExpensesPercentage = client.AdministrativeExpensesPercentage;
        BankingExpensesPercentage = client.BankingExpensesPercentage;
        MinimumBankingExpenses = client.MinimumBankingExpenses;
        Discount = discount;
        FillWeeks();
        Refresh();
    }

    private void EnsureValidWeeklyPeriod()
    {
        var days = (End - Start).TotalDays + 1;
        if (days % 7 != 0)
        {
            throw new DomainException("proforma-invalid-weekly-period");
        }
    }

    private void FillWeeks()
    {
        var weeks = ((End - Start).TotalDays + 1) / 7;
        var start = Start;
        for (int i = 1; i <= weeks; i++)
        {
            var end = start.AddDays(6);
            var week = new ProformaWeek(ProformaId, i, start, end, MinimumHours, PenaltyMinimumHours);
            Weeks.Add(week);
            start = end.AddDays(1);
        }
    }

    public void Issue(DateTimeOffset issuedAt)
    {
        EnsureTotalGreaterThenZero();
        //TODO: Validate empty weeks/work items?
        EnsureIssueAtGreaterOrEqualThanEnd(issuedAt);
        EnsureStatus(ProformaStatus.Pending);
        Status = ProformaStatus.Issued;
        IssuedAt = issuedAt;
    }

    public void Cancel(DateTimeOffset canceledAt)
    {
        EnsureStatus(ProformaStatus.Pending);
        Status = ProformaStatus.Canceled;
        CanceledAt = canceledAt;
    }

    private void EnsureIssueAtGreaterOrEqualThanEnd(DateTimeOffset issuedAt)
    {
        if (End > issuedAt)
        {
            throw new DomainException("proforma-issue-date-lower-than-end-date");
        }
    }

    public void EnsureTotalGreaterThenZero()
    {
        if (Total <= 0)
        {
            throw new DomainException("proforma-total-lower-than-zero");
        }
    }

    public void EnsureStatus(ProformaStatus status)
    {
        if (status != Status)
        {
            throw new DomainException($"proforma-status-not-{status.ToString().ToLower()}");
        }
    }

    public void AddWorkItem(int week, Guid collaboratorId, CollaboratorRole collaboratorRole, decimal hours, decimal freeHours)
    {
        EnsureStatus(ProformaStatus.Pending);

        var weekItem = Weeks.FirstOrDefault(w => w.Week == week);

        if (weekItem == null)
        {
            throw new NotFoundException<ProformaWeek>();
        }

        weekItem.Add(collaboratorId, collaboratorRole, hours, freeHours, MinimumHours, PenaltyMinimumHours);

        Refresh();
    }

    public void EditWorkItem(int week, Guid collaboratorId, decimal hours, decimal freeHours)
    {
        EnsureStatus(ProformaStatus.Pending);

        var item = Weeks.FirstOrDefault(w => w.Week == week);

        if (item == null)
        {
            throw new NotFoundException<ProformaWeek>();
        }

        item.Edit(collaboratorId, hours, freeHours, MinimumHours, PenaltyMinimumHours);

        Refresh();
    }

    public void RemoveWorkItem(int week, Guid collaboratorId)
    {
        EnsureStatus(ProformaStatus.Pending);

        var item = Weeks.FirstOrDefault(w => w.Week == week);

        if (item == null)
        {
            throw new NotFoundException<ProformaWeek>();
        }

        item.Remove(collaboratorId, MinimumHours, PenaltyMinimumHours);

        Refresh();
    }

    private void Refresh()
    {
        SubTotal = Weeks.Sum(i => i.SubTotal);

        TaxesExpensesAmount = TaxesExpensesPercentage * SubTotal / 100;

        AdministrativeExpensesAmount = AdministrativeExpensesPercentage * SubTotal / 100;

        BankingExpensesAmount = AdministrativeExpensesPercentage * SubTotal / 100;

        if (BankingExpensesAmount < MinimumBankingExpenses)
        {
            BankingExpensesAmount = MinimumBankingExpenses;
        }

        Commission = TaxesExpensesAmount + AdministrativeExpensesAmount + BankingExpensesAmount;

        Commission = Math.Round(Commission, 2, MidpointRounding.AwayFromZero);

        Total = SubTotal + Commission - Discount;
    }
}
