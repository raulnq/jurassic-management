using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;

namespace WebAPI.Collections;

public enum CollectionStatus
{
    Pending = 0,
    Confirmed = 1,
    Canceled = 2
}

public class Collection
{
    public Guid CollectionId { get; private set; }
    public Guid ClientId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public decimal Total { get; private set; }
    public CollectionStatus Status { get; private set; }
    public decimal ITF { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }
    private Collection() { }

    public Collection(Guid collectionId, Guid clientId, decimal total, Currency currency, DateTimeOffset createdAt)
    {
        CollectionId = collectionId;
        Total = total;
        CreatedAt = createdAt;
        Status = CollectionStatus.Pending;
        Currency = currency;
        ClientId = clientId;
        Refresh();
    }

    private void Refresh()
    {
        if (Total >= 1000)
        {
            ITF = 0.0005m * Total;
            ITF = Math.Round(ITF, 2, MidpointRounding.AwayFromZero);
        }
    }

    public void Confirm(decimal total, DateTime confirmedAt)
    {
        EnsureStatus(CollectionStatus.Pending);
        Total = total;
        Status = CollectionStatus.Confirmed;
        ConfirmedAt = confirmedAt;
        Refresh();
    }

    public void EnsureStatus(CollectionStatus status)
    {
        if (status != Status)
        {
            throw new DomainException($"collection-status-not-{status.ToString().ToLower()}");
        }
    }

    public void Cancel(DateTimeOffset canceledAt)
    {
        EnsureStatus(CollectionStatus.Pending);
        Status = CollectionStatus.Canceled;
        CanceledAt = canceledAt;
    }
}
