using StockMarket.Application.Common.Models;
using StockMarket.Domain.Entities;

namespace StockMarket.Application.Abstractions.Persistence;

public interface IStockCatalogRepository
{
    Task<Guid?> GetListingIdAsync(
        StockReference stock, CancellationToken cancellationToken = default);

    Task AddCompanyAsync(
        Company company, CancellationToken cancellationToken = default);

    Task AddStockListingAsync(
        StockListing listing, CancellationToken cancellationToken = default);
}
