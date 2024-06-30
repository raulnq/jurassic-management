using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;

namespace WebAPI.PayrollPayments;

public enum PayrollPaymentStatus
{
    Pending = 0,
    Paid = 1,
    AfpPaid = 2,
    Canceled = 3
}

public class PayrollPayment
{
    public Guid PayrollPaymentId { get; private set; }
    public Guid CollaboratorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? AfpPaidAt { get; private set; }
    public decimal GrossSalary { get; private set; }
    public decimal NetSalary { get; private set; }
    public decimal Afp { get; private set; }
    public decimal Commission { get; private set; }
    public PayrollPaymentStatus Status { get; private set; }
    public string? DocumentUrl { get; private set; }
    public decimal ITF { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }

    private PayrollPayment() { }

    public PayrollPayment(Guid payrollPaymentId, Guid collaboratoId, decimal netSalary, decimal afp, decimal commission, Currency currency, DateTimeOffset createdAt)
    {
        PayrollPaymentId = payrollPaymentId;
        CollaboratorId = collaboratoId;
        NetSalary = netSalary;
        Afp = afp;
        GrossSalary = netSalary + afp;
        CreatedAt = createdAt;
        Status = PayrollPaymentStatus.Pending;
        Currency = currency;
        Commission = commission;
        Refresh();
    }

    public void Edit(decimal netSalary, Currency currency, decimal afp, decimal commission)
    {
        NetSalary = netSalary;
        Afp = afp;
        GrossSalary = netSalary + afp;
        Commission = commission;
        Currency = currency;
        Refresh();
    }
    private void Refresh()
    {
        if (NetSalary >= 1000)
        {
            ITF = 0.00005m * NetSalary * 20m;
            ITF = Math.Round(ITF) / 20m;
        }
    }
    public void Pay(DateTime paidAt)
    {
        EnsureStatus(PayrollPaymentStatus.Pending);
        Status = PayrollPaymentStatus.Paid;
        PaidAt = paidAt;
    }

    public void PayAfp(DateTime paidAt)
    {
        EnsureStatus(PayrollPaymentStatus.Paid);
        Status = PayrollPaymentStatus.AfpPaid;
        AfpPaidAt = paidAt;
    }

    public void Upload(string documentUrl)
    {
        DocumentUrl = documentUrl;
    }

    public void Cancel(DateTimeOffset canceledAt)
    {
        EnsureStatus(PayrollPaymentStatus.Pending);
        Status = PayrollPaymentStatus.Canceled;
        CanceledAt = canceledAt;
    }

    public void EnsureStatus(PayrollPaymentStatus status)
    {
        if (status != Status)
        {
            throw new DomainException($"payroll-payment-status-not-{status.ToString().ToLower()}");
        }
    }
}
