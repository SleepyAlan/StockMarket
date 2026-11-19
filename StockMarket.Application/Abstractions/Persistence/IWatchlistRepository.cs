using StockMarket.Domain.Entities;

namespace StockMarket.Application.Abstractions.Persistence;

public interface IWatchlistRepository
{
    Task<Watchlist?> GetWatchlistByIdAsync(Guid watchlistId, CancellationToken cancellationToken = default);

    Task<bool> WatchlistExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(Watchlist watchlist, CancellationToken cancellationToken = default);
}
