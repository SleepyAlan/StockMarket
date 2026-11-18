using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetStockQuote;

namespace StockMarket.Application.Abstractions.StockMarketData;

public interface IQuoteProvider
{
    /// <summary>
    /// Asynchronously retrieves the stock quote for the specified stock reference.
    /// </summary>
    /// <param name="stockReference">The reference of the stock for which to retrieve the quote.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the stock quote.
    /// Return <c>null</c> if the stock quote is not available.</returns>
    Task<StockQuote?> GetStockQuoteAsync(
        StockReference stockReference, CancellationToken cancellationToken = default);
}
