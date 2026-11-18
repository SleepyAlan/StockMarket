namespace StockMarket.Domain.Entities;

public class WatchList
{
    public Guid WatchListId { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    private readonly List<WatchListItem> _items = [];

    public IReadOnlyCollection<WatchListItem> Items => _items.AsReadOnly();

    private WatchList() { }

    public WatchList(Guid userId, string name)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Invalid watchlist name: cannot be null, empty, or whitespace.", nameof(name));
        }

        WatchListId = Guid.CreateVersion7();
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

        _items.Add(new WatchListItem(WatchListId, stockListingId));
    }

    public void RemoveStock(Guid stockListingId)
    {
        var itemToRemove = _items.FirstOrDefault(item => item.StockListingId == stockListingId)
            ?? throw new InvalidOperationException("Stock listing is not in the watchlist.");
        _items.Remove(itemToRemove);
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Invalid watchlist name: cannot be null, empty, or whitespace.", nameof(newName));
        }
        Name = newName.Trim();
    }
}
