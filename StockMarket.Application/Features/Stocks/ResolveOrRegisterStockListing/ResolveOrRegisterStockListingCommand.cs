using StockMarket.Application.Common.Models;

namespace StockMarket.Application.Features.Stocks.ResolveOrRegisterStockListing;

public sealed record ResolveOrRegisterStockListingCommand(StockReference Stock);
