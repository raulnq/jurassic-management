using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;

namespace WebAPI.Invoices;

public enum InvoiceStatus
{
    Pending = 0,
    Issued = 1
}

public class Invoice
{
    public Guid InvoiceId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? IssueAt { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal Taxes { get; private set; }
    public decimal Total { get; private set; }
    public string? DocumentUrl { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public Currency Currency { get; private set; }
    private Invoice() { }

    public Invoice(Guid invoiceId, decimal subtotal, decimal taxes, Currency currency, DateTimeOffset createdAt)
    {
        InvoiceId = invoiceId;
        SubTotal = subtotal;
        Taxes = taxes;
        CreatedAt = createdAt;
        Total = subtotal + taxes;
        Currency = currency;
        Status = InvoiceStatus.Pending;
    }

    public void UploadDocument(string documentUrl)
    {
        DocumentUrl = documentUrl;
    }

    public void Issue(DateTime issueAt)
    {
        EnsureDocumentIsNotEmpty();
        Status = InvoiceStatus.Issued;
        IssueAt = issueAt;
    }

    private void EnsureDocumentIsNotEmpty()
    {
        if (string.IsNullOrEmpty(DocumentUrl))
        {
            throw new DomainException("invoice-document-is-empty");
        }
    }
}
