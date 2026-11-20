namespace StockMarket.Application.Features.Stocks.GetHistoricalPrices;

public sealed record HistoricalPriceBar(
    DateTimeOffset TimestampUtc,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume);
