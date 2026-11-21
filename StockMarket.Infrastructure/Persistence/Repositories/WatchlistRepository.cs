using Microsoft.EntityFrameworkCore;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Domain.Entities;

namespace StockMarket.Infrastructure.Persistence.Repositories;

public sealed class WatchlistRepository : IWatchlistRepository
{
    private readonly ApplicationDbContext _db;

    public WatchlistRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Watchlist watchlist, CancellationToken cancellationToken = default)
    {
        await _db.Watchlists.AddAsync(watchlist, cancellationToken);
    }

    public async Task<Watchlist?> GetWatchlistByIdAsync(Guid watchlistId, CancellationToken cancellationToken = default)
    {
        return await _db.Watchlists
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.WatchlistId == watchlistId, cancellationToken);
    }

    public async Task<bool> WatchlistExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken = default)
    {
        return await _db.Watchlists.AnyAsync(x => x.UserId == userId && x.Name == name, cancellationToken);
    }
}
