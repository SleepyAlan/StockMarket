using StockMarket.Application.Common.Models;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Application.Abstractions.Persistence;

public interface IListingCurrencyReader
{
    Task<CurrencyCode?> GetCurrencyAsync(StockReference stock, CancellationToken cancellationToken = default);
}
