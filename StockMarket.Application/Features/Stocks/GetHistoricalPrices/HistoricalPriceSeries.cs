using StockMarket.Application.Common.Models;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Application.Features.Stocks.GetHistoricalPrices;

public sealed record HistoricalPriceSeries(
    StockReference Stock,
    CurrencyCode Currency,
    HistoricalPriceInterval Interval,
    PriceAdjustmentMode Adjustment,
    string Provider,
    MarketDataFeed Feed,
    IReadOnlyList<HistoricalPriceBar> Bars);
