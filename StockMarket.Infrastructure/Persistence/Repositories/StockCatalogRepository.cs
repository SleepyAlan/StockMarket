using Microsoft.EntityFrameworkCore;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Application.Common.Models;
using StockMarket.Domain.Entities;

namespace StockMarket.Infrastructure.Persistence.Repositories;

public sealed class StockCatalogRepository : IStockCatalogRepository
{
    private readonly ApplicationDbContext _db;

    public StockCatalogRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddCompanyAsync(
        Company company, CancellationToken cancellationToken = default)
    {
        await _db.Companies.AddAsync(company, cancellationToken);
    }

    public async Task AddStockListingAsync(
        StockListing listing, CancellationToken cancellationToken = default)
    {
        await _db.StockListings.AddAsync(listing, cancellationToken);
    }

    public async Task<Guid?> GetListingIdAsync(
        StockReference stock, CancellationToken cancellationToken = default)
    {
        return await _db.StockListings
            .AsNoTracking()
            .Where(x => x.Exchange == stock.Exchange && x.Symbol == stock.Symbol)
            .Select(x => x.StockListingId)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
