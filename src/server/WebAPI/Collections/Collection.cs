using WebAPI.Proformas;

namespace WebAPI.Collections;

public enum CollectionStatus
{
    Pending = 0,
    Confirmed = 1
}

public class Collection
{
    public Guid CollectionId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public decimal Total { get; private set; }
    public CollectionStatus Status { get; private set; }
    public decimal ITF { get; private set; }
    public Currency Currency { get; private set; }
    private Collection() { }

    public Collection(Guid collectionId, decimal total, Currency currency, DateTimeOffset createdAt)
    {
        CollectionId = collectionId;
        Total = total;
        CreatedAt = createdAt;
        Status = CollectionStatus.Pending;
        Currency = currency;
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
        Total = total;
        Status = CollectionStatus.Confirmed;
        ConfirmedAt = confirmedAt;
        Refresh();
    }
}
