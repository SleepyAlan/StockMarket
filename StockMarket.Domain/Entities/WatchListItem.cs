namespace StockMarket.Domain.Entities;

public class WatchListItem
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
}
