using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;
using Infrastructure;

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
    public DateTime? IssuedAt { get; private set; }
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

    public void Issue(DateTime issuedAt, string number)
    {
        EnsureStatus(InvoiceStatus.Pending);
        Status = InvoiceStatus.Issued;
        IssuedAt = issuedAt;
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

    public class EntityTypeConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder
                .ToTable(Tables.Invoices);

            builder
                .HasKey(c => c.InvoiceId);

            builder
                .Property(c => c.Total)
                .HasColumnType("decimal(19, 4)");

            builder
                .Property(c => c.SubTotal)
                .HasColumnType("decimal(19, 4)");

            builder
                .Property(c => c.Taxes)
                .HasColumnType("decimal(19, 4)");

            builder
                .Property(c => c.Currency)
                .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());

            builder
                .Property(c => c.Status)
                .HasConversion(s => s.ToString(), value => value.ToEnum<InvoiceStatus>());
        }
    }
}
