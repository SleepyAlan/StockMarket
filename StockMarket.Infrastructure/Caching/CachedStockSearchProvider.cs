using Microsoft.Extensions.Caching.Memory;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Features.Stocks.SearchStocks;
using System.Reflection;

namespace StockMarket.Infrastructure.Caching;

public sealed class CachedStockSearchProvider :IStockSearchProvider
{
    private readonly IStockSearchProvider _stockSearchProvider;
    private readonly IMemoryCache _cache;


    public CachedStockSearchProvider(
        IStockSearchProvider stockSearchProvider, 
        IMemoryCache cache)
    {
        _stockSearchProvider = stockSearchProvider;
        _cache = cache;
    }

    public async Task<IReadOnlyList<StockSearchResult>> SearchStocksAsync(
        string searchTerm, CancellationToken cancellationToken = default)
    {
        string normalized = searchTerm.Trim().ToUpperInvariant();
        string cacheKey = $"stocks:search:{normalized}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<StockSearchResult>? cached) &&
            cached is not null)
        {
            return cached;
        }

        var results = await _stockSearchProvider
            .SearchStocksAsync(searchTerm, cancellationToken);

        _cache.Set(cacheKey, results, TimeSpan.FromMinutes(30));

        return results;
    }
}
