namespace StockMarket.Domain.Entities;

public class Watchlist
{
    public Guid WatchlistId { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    private readonly List<WatchlistItem> _items = [];

    public IReadOnlyCollection<WatchlistItem> Items => _items.AsReadOnly();

    private Watchlist() { }

    public Watchlist(Guid userId, string name)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        WatchlistId = Guid.CreateVersion7();
        UserId = userId;
        Name = name.Trim();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void AddStock(Guid stockListingId)
    {
        if (stockListingId == Guid.Empty)
        {
            throw new ArgumentException("Stock listing ID cannot be empty.", nameof(stockListingId));
        }
        if (_items.Any(item => item.StockListingId == stockListingId))
        {
            throw new InvalidOperationException("Stock listing is already in the watchlist.");
        }

        _items.Add(new WatchlistItem(WatchlistId, stockListingId));
    }

    public void RemoveStock(Guid stockListingId)
    {
        var itemToRemove = _items.FirstOrDefault(item => item.StockListingId == stockListingId)
            ?? throw new InvalidOperationException("Stock listing is not in the watchlist.");
        _items.Remove(itemToRemove);
    }

    public void Rename(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName, nameof(newName));
        Name = newName.Trim();
    }
}
