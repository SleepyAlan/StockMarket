using StockMarket.Application.Common.Models;

namespace StockMarket.Application.Features.Stocks.GetStockQuote;

public sealed record GetStockQuoteQuery(StockReference Stock);
