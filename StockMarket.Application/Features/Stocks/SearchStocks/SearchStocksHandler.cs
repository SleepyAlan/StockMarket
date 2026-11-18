using StockMarket.Application.Abstractions.StockMarketData;

namespace StockMarket.Application.Features.Stocks.SearchStocks;

public sealed class SearchStocksHandler
{
    private readonly IStockSearchProvider _stockSearchProvider;

    public SearchStocksHandler(IStockSearchProvider stockSearchProvider)
    {
        _stockSearchProvider = stockSearchProvider;
    }

    public async Task<IReadOnlyList<StockSearchResult>> HandleAsync(
        SearchStocksQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        string searchTerm = query.SearchTerm?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new ArgumentException("Search term cannot be null or empty.");
        }
        if (searchTerm.Length > 255)
        {
            throw new ArgumentException("Search term cannot exceed 255 characters.");
        }

        return await _stockSearchProvider.SearchStocksAsync(searchTerm, cancellationToken);
    }
}
