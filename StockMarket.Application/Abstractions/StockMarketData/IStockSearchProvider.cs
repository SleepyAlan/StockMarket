using StockMarket.Application.Features.Stocks.SearchStocks;

namespace StockMarket.Application.Abstractions.StockMarketData;

public interface IStockSearchProvider
{
    /// <summary>
    /// Asynchronously searches for stocks that match the specified search term.
    /// </summary>
    /// <param name="searchTerm">Text used to match stocks by symbol, name, or other searchable fields.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of matching stock
    /// search results.</returns>
    Task<IReadOnlyList<StockSearchResult>> SearchStocksAsync(
        string searchTerm, CancellationToken cancellationToken = default);
}
