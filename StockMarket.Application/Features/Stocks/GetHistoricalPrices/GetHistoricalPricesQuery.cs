using StockMarket.Application.Common.Models;

namespace StockMarket.Application.Features.Stocks.GetHistoricalPrices;

public sealed record GetHistoricalPricesQuery(
    StockReference Stock,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    HistoricalPriceInterval Interval,
    PriceAdjustmentMode Adjustment = PriceAdjustmentMode.Raw);
