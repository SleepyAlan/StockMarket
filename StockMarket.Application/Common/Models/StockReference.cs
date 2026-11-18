using StockMarket.Domain.ValueObjects;

namespace StockMarket.Application.Common.Models;

public sealed record StockReference(
    TickerSymbol Symbol,
    ExchangeCode Exchange);
