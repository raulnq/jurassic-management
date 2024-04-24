using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;

namespace WebAPI.CollaboratorPayments;

public enum CollaboratorPaymentStatus
{
    Pending = 0,
    Paid = 1,
    Confirmed = 2
}

public class CollaboratorPayment
{
    public Guid CollaboratorPaymentId { get; private set; }
    public Guid ProformaId { get; private set; }
    public int Week { get; private set; }
    public Guid CollaboratorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public decimal GrossSalary { get; private set; }
    public decimal NetSalary { get; private set; }
    public decimal Withholding { get; private set; }
    public CollaboratorPaymentStatus Status { get; private set; }
    public string? DocumentUrl { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public decimal ITF { get; private set; }
    public Currency Currency { get; private set; }

    private CollaboratorPayment() { }

    public CollaboratorPayment(Guid collaboratorPaymentId, Guid proformaId, int week, Guid collaboratoId, decimal grossSalary, decimal withholdingPercentage, Currency currency, DateTimeOffset createdAt)
    {
        CollaboratorPaymentId = collaboratorPaymentId;
        ProformaId = proformaId;
        Week = week;
        CollaboratorId = collaboratoId;
        GrossSalary = grossSalary;
        Withholding = (grossSalary * withholdingPercentage) / 100;
        CreatedAt = createdAt;
        NetSalary = GrossSalary - Withholding;
        Status = CollaboratorPaymentStatus.Pending;
        Currency = currency;
        Refresh();
    }
    private void Refresh()
    {
        if (NetSalary >= 1000)
        {
            ITF = 0.0005m * NetSalary;
            ITF = Math.Round(ITF, 2, MidpointRounding.AwayFromZero);
        }
    }
    public void Paid(DateTime paidAt)
    {
        Status = CollaboratorPaymentStatus.Paid;
        PaidAt = paidAt;
    }

    public void Upload(string documentUrl)
    {
        EnsureItIsPaid();
        Status = CollaboratorPaymentStatus.Confirmed;
        DocumentUrl = documentUrl;
    }

    public void Confirm(DateTime confirmedAt)
    {
        EnsureDocumentIsNotEmpty();
        Status = CollaboratorPaymentStatus.Confirmed;
        ConfirmedAt = confirmedAt;
    }

    private void EnsureItIsPaid()
    {
        if (Status != CollaboratorPaymentStatus.Paid)
        {
            throw new DomainException("collaborator-payment-is-not-paid");
        }
    }

    private void EnsureDocumentIsNotEmpty()
    {
        if (string.IsNullOrEmpty(DocumentUrl))
        {
            throw new DomainException("collaborator-payment-document-is-empty");
        }
    }
}
