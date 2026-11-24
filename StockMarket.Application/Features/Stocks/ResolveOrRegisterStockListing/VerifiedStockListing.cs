using StockMarket.Application.Common.Models;
using StockMarket.Domain.ValueObjects;
namespace StockMarket.Application.Features.Stocks.ResolveOrRegisterStockListing;

public sealed record VerifiedStockListing(
    StockReference Stock,
    CurrencyCode Currency,
    string ProviderSymbol,
    string Provider);
