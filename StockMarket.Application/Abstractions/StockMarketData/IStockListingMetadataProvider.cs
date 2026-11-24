using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.ResolveOrRegisterStockListing;

namespace StockMarket.Application.Abstractions.StockMarketData;

public interface IStockListingMetadataProvider
{
    /// <summary>
    /// Resolves a stock reference to a verified stock listing asynchronously.
    /// </summary>
    /// <param name="stock">The stock reference to resolve.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the verified stock listing if
    /// resolution succeeds; otherwise, <see langword="null"/>.</returns>
    Task<VerifiedStockListing?> ResolveAsync(
        StockReference stock, CancellationToken cancellationToken = default);
}
