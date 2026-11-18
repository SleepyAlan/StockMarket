using StockMarket.Application.Common.Models;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Application.Features.Stocks.GetStockQuote;

public sealed record StockQuote(
    StockReference Stock,
    Money Price,
    Money? PreviousClose,
    DateTimeOffset AtThisTimeUtc,
    string Provider,
    MarketDataDelay Delay);
