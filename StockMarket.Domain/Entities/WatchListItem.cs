namespace StockMarket.Domain.Entities;

public class WatchlistItem : IEquatable<WatchlistItem>
{
    public Guid WatchlistItemId { get; private set; }
    public Guid WatchlistId { get; private set; }
    public Guid StockListingId { get; private set; }
    public DateTimeOffset AddedAtUtc { get; private set; }

    private WatchlistItem() { }

    internal WatchlistItem(Guid watchlistId, Guid stockListingId)
    {
        if (watchlistId == Guid.Empty)
        {
            throw new ArgumentException("Invalid watchlist ID: cannot be empty.", nameof(watchlistId));
        }
        if (stockListingId == Guid.Empty)
        {
            throw new ArgumentException("Invalid stock listing ID: cannot be empty.", nameof(stockListingId));
        }

        WatchlistItemId = Guid.CreateVersion7();
        WatchlistId = watchlistId;
        StockListingId = stockListingId;
        AddedAtUtc = DateTimeOffset.UtcNow;
    }

    public bool Equals(WatchlistItem? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return WatchlistItemId == other.WatchlistItemId;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as WatchlistItem);
    }

    public override int GetHashCode()
    {
        return WatchlistItemId.GetHashCode();
    }
}
