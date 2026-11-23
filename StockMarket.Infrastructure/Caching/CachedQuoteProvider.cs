using Microsoft.Extensions.Caching.Memory;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetStockQuote;

namespace StockMarket.Infrastructure.Caching;

public sealed class CachedQuoteProvider : IQuoteProvider
{
    private readonly IQuoteProvider _quoteProvider;
    private readonly IMemoryCache _cache;

    public CachedQuoteProvider(
        IQuoteProvider quoteProvider,
        IMemoryCache cache)
    {
        _quoteProvider = quoteProvider;
        _cache = cache;
    }

    public async Task<StockQuote?> GetStockQuoteAsync(
        StockReference stockReference, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"stocks:quote:{stockReference.Symbol}:{stockReference.Exchange}";

        if (_cache.TryGetValue(cacheKey, out StockQuote? cached) && cached is not null)
        {
            return cached;
        }

        var quote = await _quoteProvider.GetStockQuoteAsync(stockReference, cancellationToken);

        if (quote is not null)
        {
            _cache.Set(cacheKey, quote, TimeSpan.FromSeconds(15));
        }

        return quote;
    }
}
