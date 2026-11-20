using StockMarket.Application.Features.Watchlists.GetWatchlists;

namespace StockMarket.Application.Abstractions.Persistence;

public interface IWatchlistQueries
{
    Task<IReadOnlyList<WatchlistSummary>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
