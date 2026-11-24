using Microsoft.Extensions.Caching.Memory;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.ResolveOrRegisterStockListing;
using StockMarket.Domain.ValueObjects;
using StockMarket.Infrastructure.StockMarketData.Finnhub.DTOs;

namespace StockMarket.Infrastructure.StockMarketData.Finnhub.Providers;

public sealed class FinnhubStockListingMetadataProvider : IStockListingMetadataProvider
{
    private readonly FinnhubClient _client;
    private readonly IMemoryCache _cache;

    //TODO: verify and add more markets
    private static readonly IReadOnlyDictionary<string, string> SupportedMarkets = new Dictionary<string, string>()
    {
        ["XNAS"] = "US",
        ["XNYS"] = "US"
    };

    public FinnhubStockListingMetadataProvider(FinnhubClient client, IMemoryCache cache)
    {
        _client = client;
        _cache = cache;
    }

    public async Task<VerifiedStockListing?> ResolveAsync(
        StockReference stock, CancellationToken cancellationToken = default)
    {
        if (!SupportedMarkets.TryGetValue(stock.Exchange.Value, out var market))
        {
            throw new NotSupportedException("Exchange is not supported.");
        }

        string mic = stock.Exchange.Value;
        var cacheKey = $"finnhub:symbols:{market}:{mic}";

        var symbols = await _cache
            .GetOrCreateAsync(cacheKey, async entry =>
            {
                string path =
                    $"stock/symbol?exchange={market}" +
                    $"&mic={Uri.EscapeDataString(mic)}";

                var result = await _client
                    .GetAsync<List<FinnhubStockSymbolDto>>(path, cancellationToken)
                    ?? throw new InvalidOperationException(
                        "The stock catalog provider returned no response.");

                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
                return result;

            })
            ?? throw new InvalidOperationException("Stock catalog is unavailable.");

        var matches = symbols
            .Where(x =>
                string.Equals(
                    x.Mic,
                    mic,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    string.IsNullOrWhiteSpace(x.DisplaySymbol) ? x.Symbol : x.DisplaySymbol,
                    stock.Symbol.Value,
                    StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .ToArray();

        if (matches.Length == 0)
        {
            return null;
        }
            

        if (matches.Length > 1)
        {
            throw new InvalidOperationException(
                "Multiple matching stock listings were found.");
        }

        var match = matches[0];

        if (string.IsNullOrWhiteSpace(match.Currency) ||
            string.IsNullOrWhiteSpace(match.Symbol))
        {
            throw new InvalidOperationException(
                "Required listing metadata is missing.");
        }

        if (!string.Equals(match.Symbol, stock.Symbol.Value,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(
                "This listing requires provider-specific symbol mapping.");
        }

        return new VerifiedStockListing(
            Stock: stock,
            Currency: new CurrencyCode(match.Currency),
            ProviderSymbol: match.Symbol,
            Provider: "Finnhub");
    }
}
