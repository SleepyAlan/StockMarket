using Microsoft.Extensions.Logging;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetStockQuote;
using StockMarket.Domain.ValueObjects;
using StockMarket.Infrastructure.StockMarketData.Finnhub.DTOs;
using System.Collections;

namespace StockMarket.Infrastructure.StockMarketData.Finnhub.Providers;

public sealed class FinnhubQuoteProvider : IQuoteProvider
{
    private readonly FinnhubClient _finnhubClient;
    private readonly IListingCurrencyReader _listingCurrencyReader;
    private readonly ILogger<FinnhubQuoteProvider> _logger;

    public FinnhubQuoteProvider(
        FinnhubClient finnhubClient, 
        IListingCurrencyReader listingCurrencyReader,
        ILogger<FinnhubQuoteProvider> logger)
    {
        _finnhubClient = finnhubClient;
        _listingCurrencyReader = listingCurrencyReader;
        _logger = logger;
    }

    public async Task<StockQuote?> GetStockQuoteAsync(StockReference stockReference, CancellationToken cancellationToken = default)
    {
        var currency =  await _listingCurrencyReader.GetCurrencyAsync(stockReference, cancellationToken);

        if (currency is null)
        {
            _logger.LogWarning("Could not find listing currency for stock {StockReference}", stockReference);
            return null;
        }

        string relativePath = $"quote?symbol={Uri.EscapeDataString(stockReference.Symbol.Value)}";

        var response =
           await _finnhubClient.GetAsync<FinnhubQuoteResponse>(
               relativePath,
               cancellationToken);

        if (response?.Current is not > 0 ||
            response.Timestamp is null or <= 0)
        {
            return null;
        }

        var price = new Money(response.Current.Value, currency);

        var previousClose = response.PreviousClose is > 0
                ? new Money(response.PreviousClose.Value, currency)
                : null;

        return new StockQuote(
            Stock: stockReference,
            Price: price,
            PreviousClose: previousClose,
            AtThisTimeUtc: DateTimeOffset.FromUnixTimeSeconds(response.Timestamp.Value),
            Provider: "Finnhub",
            Delay: MarketDataDelay.Unknown);
    }
}
