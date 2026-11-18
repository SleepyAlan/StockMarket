using StockMarket.Domain.ValueObjects;

namespace StockMarket.Application.Features.Stocks.SearchStocks;

public sealed record StockSearchResult(
    TickerSymbol Symbol,
    string CompanyName,
    ExchangeCode? Exchange,
    CurrencyCode? Currency);
