namespace StockMarket.Domain.Entities;

public class WatchListItem : IEquatable<WatchListItem>
{
    public Guid WatchListItemId { get; private set; }
    public Guid WatchListId { get; private set; }
    public Guid StockListingId { get; private set; }
    public DateTimeOffset AddedAtUtc { get; private set; }

    private WatchListItem() { }

    internal WatchListItem(Guid watchListId, Guid stockListingId)
    {
        if (watchListId == Guid.Empty)
        {
            throw new ArgumentException("Invalid watchlist ID: cannot be empty.", nameof(watchListId));
        }
        if (stockListingId == Guid.Empty)
        {
            throw new ArgumentException("Invalid stock listing ID: cannot be empty.", nameof(stockListingId));
        }

        WatchListItemId = Guid.CreateVersion7();
        WatchListId = watchListId;
        StockListingId = stockListingId;
        AddedAtUtc = DateTimeOffset.UtcNow;
    }

    public bool Equals(WatchListItem? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return WatchListItemId == other.WatchListItemId;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as WatchListItem);
    }

    public override int GetHashCode()
    {
        return WatchListItemId.GetHashCode();
    }
}
