namespace StockMarket.Application.Features.Watchlists.RemoveStockFromWatchlist;

public sealed record RemoveStockFromWatchlistCommand(Guid WatchlistId, Guid StockListingId);
