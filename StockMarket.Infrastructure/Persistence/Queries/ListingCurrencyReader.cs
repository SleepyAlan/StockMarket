using Microsoft.EntityFrameworkCore;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Application.Common.Models;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Infrastructure.Persistence.Queries;

public sealed class ListingCurrencyReader : IListingCurrencyReader
{
    private readonly ApplicationDbContext _db;

    public ListingCurrencyReader(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CurrencyCode?> GetCurrencyAsync(
        StockReference stock,
        CancellationToken cancellationToken = default)
    {
        return await _db.StockListings
            .AsNoTracking()
            .Where(x => x.Symbol == stock.Symbol && x.Exchange == stock.Exchange)
            .Select(x => x.Currency)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
