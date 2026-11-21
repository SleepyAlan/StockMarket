using Microsoft.EntityFrameworkCore;
using StockMarket.Application.Abstractions.Persistence;

namespace StockMarket.Infrastructure.Persistence.Queries;

public sealed class StockListingReader : IStockListingReader
{
    private readonly ApplicationDbContext _db;

    public StockListingReader(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> StockListingExistsByIdAsync(Guid stockListingId, CancellationToken cancellationToken = default)
    {
        return await _db.StockListings
            .AnyAsync(s => s.StockListingId == stockListingId, cancellationToken);
    }
}
