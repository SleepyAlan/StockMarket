namespace StockMarket.Application.Features.Watchlists.GetWatchlists;

public sealed record WatchlistSummary(
    Guid Id,
    string Name,
    int StockCount,
    DateTimeOffset CreatedAtUtc);
