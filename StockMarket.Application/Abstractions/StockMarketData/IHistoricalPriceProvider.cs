using StockMarket.Application.Features.Stocks.GetHistoricalPrices;

namespace StockMarket.Application.Abstractions.StockMarketData;

public interface IHistoricalPriceProvider
{
    Task<HistoricalPriceSeries?> GetHistoricalPricesAsync(
        GetHistoricalPricesQuery query,
        CancellationToken cancellationToken = default);
}
