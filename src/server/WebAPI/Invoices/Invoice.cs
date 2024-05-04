using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;

namespace WebAPI.Invoices;

public enum InvoiceStatus
{
    Pending = 0,
    Issued = 1,
    Canceled = 2
}

public class Invoice
{
    public Guid InvoiceId { get; private set; }
    public Guid ClientId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? IssueAt { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal Taxes { get; private set; }
    public decimal Total { get; private set; }
    public string? DocumentUrl { get; private set; }
    public string? Number { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }
    private Invoice() { }

    public Invoice(Guid invoiceId, Guid clientId, decimal subtotal, decimal taxes, Currency currency, DateTimeOffset createdAt)
    {
        InvoiceId = invoiceId;
        SubTotal = subtotal;
        Taxes = taxes;
        CreatedAt = createdAt;
        Total = subtotal + taxes;
        Currency = currency;
        Status = InvoiceStatus.Pending;
        ClientId = clientId;
    }

    public void UploadDocument(string documentUrl)
    {
        DocumentUrl = documentUrl;
    }

    public void Issue(DateTime issueAt, string number)
    {
        EnsureDocumentIsNotEmpty();
        EnsureStatus(InvoiceStatus.Pending);
        Status = InvoiceStatus.Issued;
        IssueAt = issueAt;
        Number = number;
    }

    public void Cancel(DateTimeOffset canceledAt)
    {
        EnsureStatus(InvoiceStatus.Pending);
        Status = InvoiceStatus.Canceled;
        CanceledAt = canceledAt;
    }

    public void EnsureStatus(InvoiceStatus status)
    {
        if (status != Status)
        {
            throw new DomainException($"invoice-status-not-{status.ToString().ToLower()}");
        }
    }

    private void EnsureDocumentIsNotEmpty()
    {
        if (string.IsNullOrEmpty(DocumentUrl))
        {
            throw new DomainException("invoice-document-is-empty");
        }
    }
}
