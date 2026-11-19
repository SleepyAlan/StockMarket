namespace StockMarket.Application.Features.Watchlists.AddStockToWatchlist;

public sealed record AddStockToWatchlistCommand(Guid WatchlistId, Guid StockListingId);
