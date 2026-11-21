using Microsoft.EntityFrameworkCore;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Application.Features.Watchlists.GetWatchlists;

namespace StockMarket.Infrastructure.Persistence.Queries;

public sealed class WatchlistQueries : IWatchlistQueries
{
    private readonly ApplicationDbContext _db;

    public WatchlistQueries(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<WatchlistSummary>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.Watchlists
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new WatchlistSummary(x.WatchlistId, x.Name, x.Items.Count, x.CreatedAtUtc))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
