using Microsoft.Extensions.Logging;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Features.Stocks.SearchStocks;
using StockMarket.Domain.ValueObjects;
using StockMarket.Infrastructure.StockMarketData.Finnhub.DTOs;

namespace StockMarket.Infrastructure.StockMarketData.Finnhub.Providers;

public sealed class FinnhubStockSearchProvider : IStockSearchProvider
{
    private readonly FinnhubClient _client;
    private readonly ILogger<FinnhubStockSearchProvider> _logger;

    public FinnhubStockSearchProvider(FinnhubClient client, ILogger<FinnhubStockSearchProvider> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StockSearchResult>> SearchStocksAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        string relativePath = $"search?q={Uri.EscapeDataString(searchTerm)}";

        var response = await _client.GetAsync<FinnhubSearchResponse>(relativePath, cancellationToken);

        var results = new List<StockSearchResult>();

        foreach(var result in response?.Result ?? [])
        {
            if (string.IsNullOrWhiteSpace(result.Description) ||
                string.IsNullOrWhiteSpace(result.Symbol)) continue;

            try
            {
                var stockSearchResult = new StockSearchResult(
                    Symbol: new TickerSymbol(result.Symbol),
                    CompanyName: result.Description,
                    Exchange: null,
                    Currency: null);
                results.Add(stockSearchResult);
            }
            catch(ArgumentException) 
            {
                _logger.LogInformation("Unsupported symbol: {Symbol} from {Provider}",
                    result.Symbol, "Finnhub");
            } 
        }

        return results;
    }
}
