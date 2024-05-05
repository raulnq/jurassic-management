using WebAPI.Proformas;

namespace WebAPI.Transactions;

public enum TransactionType
{
    Income = 0,
    Expenses = 1
}

public class Transaction
{
    public Guid TransactionId { get; private set; }
    public string Description { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal Taxes { get; private set; }
    public decimal Total { get; private set; }
    public decimal ITF { get; private set; }
    public string? Number { get; private set; }
    public string? DocumentUrl { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public TransactionType Type { get; private set; }

    private Transaction() { }

    public Transaction(Guid transactionId,
        TransactionType type,
        string description,
        decimal subTotal,
        decimal taxes,
        Currency currency,
        string? number,
        DateTime issuedAt,
        DateTimeOffset createdAt)
    {
        TransactionId = transactionId;
        Type = type;
        Description = description;
        SubTotal = subTotal;
        Taxes = taxes;
        Total = subTotal + taxes;
        CreatedAt = createdAt;
        Number = number;
        Currency = currency;
        IssuedAt = issuedAt;
        Refresh();
    }

    public void Edit(
        TransactionType type,
        string description,
        decimal subTotal,
        decimal taxes,
        Currency currency,
        string? number,
        DateTime issuedAt)
    {
        Type = type;
        Description = description;
        SubTotal = subTotal;
        Taxes = taxes;
        Total = subTotal + taxes;
        Number = number;
        Currency = currency;
        IssuedAt = issuedAt;
        Refresh();
    }

    public void UploadDocument(string documentUrl)
    {
        DocumentUrl = documentUrl;
    }

    private void Refresh()
    {
        if (Total >= 1000)
        {
            ITF = 0.0005m * Total;
            ITF = Math.Round(ITF, 2, MidpointRounding.AwayFromZero);
        }
    }
}
