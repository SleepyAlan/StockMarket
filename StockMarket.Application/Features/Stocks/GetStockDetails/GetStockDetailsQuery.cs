using StockMarket.Application.Common.Models;

namespace StockMarket.Application.Features.Stocks.GetStockDetails;

public sealed record GetStockDetailsQuery(StockReference Stock);
