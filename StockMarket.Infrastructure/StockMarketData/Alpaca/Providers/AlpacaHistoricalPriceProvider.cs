using Microsoft.Extensions.Logging;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Features.Stocks.GetHistoricalPrices;
using StockMarket.Infrastructure.StockMarketData.Alpaca.DTOs;

namespace StockMarket.Infrastructure.StockMarketData.Alpaca.Providers;

public sealed class AlpacaHistoricalPriceProvider : IHistoricalPriceProvider
{
    private readonly AlpacaClient _client;
    private readonly IListingCurrencyReader _currencyReader;
    private readonly ILogger<AlpacaHistoricalPriceProvider> _logger;

    public AlpacaHistoricalPriceProvider(AlpacaClient client, IListingCurrencyReader currencyReader, ILogger<AlpacaHistoricalPriceProvider> logger)
    {
        _client = client;
        _currencyReader = currencyReader;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously retrieves historical SIP price bars for the specified stock and returns them as a historical
    /// price series.
    /// </summary>
    /// <remarks>Requests data from the SIP feed, paginates through provider results, and includes only bars
    /// within the requested UTC range.</remarks>
    /// <param name="query">Specifies the stock, time range, interval, and adjustment settings used to request historical bars.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be canceled.</param>
    /// <returns>A task that resolves to a historical price series when data is available; otherwise <see langword="null"/> if
    /// the listing currency cannot be resolved or the data response is unavailable.</returns>
    /// <exception cref="ArgumentException">Thrown when <c>query.ToUtc</c> is more recent than the minimum allowed SIP delay window.</exception>
    public async Task<HistoricalPriceSeries?> GetHistoricalPricesAsync(GetHistoricalPricesQuery query, CancellationToken cancellationToken = default)
    {
        var currency = await _currencyReader.GetCurrencyAsync(query.Stock, cancellationToken);

        if (currency is null)
        {
            _logger.LogWarning("Could not find listing currency for stock {StockReference}.", query.Stock);
            return null;
        }

        var latestAllowedEnd = DateTimeOffset.UtcNow.AddMinutes(-16);
        if (query.ToUtc > latestAllowedEnd) throw new ArgumentException("Historical SIP data must be at least 15 minutes old.");
        

        string timeframe = RetrieveTimeframe(query);
        string adjustment = RetrieveAdjustment(query);

        // https://docs.alpaca.markets/us/reference/stockbars
        // Alpaca API may return less if there are more than 1000 bars,
        // so we need to paginate through the results if there are more than 1000 bars.
        var bars = new List<HistoricalPriceBar>();
        string? nextPageToken = null;
        do
        {
            string relativePath =
                $"stocks/{Uri.EscapeDataString(query.Stock.Symbol.Value)}/bars" +
                $"?timeframe={timeframe}" +
                $"&start={Uri.EscapeDataString(query.FromUtc.ToString("O"))}" +
                $"&end={Uri.EscapeDataString(query.ToUtc.ToString("O"))}" +
                $"&adjustment={adjustment}" +
                $"&feed=sip" +
                $"&limit=1000" +
                $"&sort=asc";

            if (!string.IsNullOrEmpty(nextPageToken))
            {
                relativePath += $"&page_token={Uri.EscapeDataString(nextPageToken)}";
            }

            var response = await _client
                .GetAsync<AlpacaHistoricalBarsResponse>(relativePath, cancellationToken);

            if (response is null) return null;

            foreach (var bar in response.Bars ?? [])
            {
                if (bar.Timestamp < query.FromUtc ||bar.Timestamp >= query.ToUtc)
                {
                    continue;
                }

                bars.Add(new HistoricalPriceBar(
                    TimestampUtc: bar.Timestamp.ToUniversalTime(),
                    Open: bar.Open,
                    High: bar.High,
                    Low: bar.Low,
                    Close: bar.Close,
                    Volume: bar.Volume));
            }

            nextPageToken = response.NextPageToken;

        } while (!string.IsNullOrEmpty(nextPageToken));

        return new HistoricalPriceSeries(
            Stock: query.Stock,
            Currency: currency,
            Interval: query.Interval,
            Adjustment: query.Adjustment,
            Provider: "Alpaca",
            Feed: MarketDataFeed.Sip,
            Bars: bars);

    }

    #region Private Helpers

    private static string RetrieveAdjustment(GetHistoricalPricesQuery query) =>
        query.Adjustment switch
        {
            PriceAdjustmentMode.Raw => "raw",
            PriceAdjustmentMode.SplitAdjusted => "split",
            PriceAdjustmentMode.DividendAdjusted => "dividend",
            PriceAdjustmentMode.SpinOffAdjusted => "spin-off",
            PriceAdjustmentMode.SplitDividendAndSpinOffAdjusted => "all",
            _ => throw new NotSupportedException("Unsupported price adjustment mode.")
        };

    private static string RetrieveTimeframe(GetHistoricalPricesQuery query) => 
        query.Interval switch
        {
            HistoricalPriceInterval.OneMinute => "1Min",
            HistoricalPriceInterval.FiveMinutes => "5Min",
            HistoricalPriceInterval.FifteenMinutes => "15Min",
            HistoricalPriceInterval.OneHour => "1Hour",
            HistoricalPriceInterval.OneDay => "1Day",
            HistoricalPriceInterval.OneWeek => "1Week",
            HistoricalPriceInterval.OneMonth => "1Month",
            HistoricalPriceInterval.ThreeMonths => "3Month",
            HistoricalPriceInterval.SixMonths => "6Month",
            HistoricalPriceInterval.OneYear => "12Month",
            _ => throw new NotSupportedException("Unsupported historical price interval.")
        };

    #endregion
}
