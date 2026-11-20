using StockMarket.Application.Abstractions.StockMarketData;

namespace StockMarket.Application.Features.Stocks.GetHistoricalPrices;

public sealed class GetHistoricalPricesHandler
{
    private readonly IHistoricalPriceProvider _historicalPriceProvider;
    
    public GetHistoricalPricesHandler(IHistoricalPriceProvider historicalPriceProvider)
    {
        _historicalPriceProvider = historicalPriceProvider;
    }
    
    /// <summary>
    /// Retrieves historical price data for the specified stock and time range,
    /// normalizes timestamps to UTC, and returns bars ordered by timestamp.
    /// </summary>
    /// <remarks>The query date range is converted to UTC before requesting data from the provider.</remarks>
    /// <param name="query">Query containing the stock, date range, interval, and adjustment settings.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the historical price series with bars
    /// sorted in ascending UTC timestamp order, or <see langword="null"/> when no data is available.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="query"/> contains an undefined interval or adjustment value.</exception>
    /// <exception cref="ArgumentException">Thrown when the start date is not earlier than the end date.</exception>
    public async Task<HistoricalPriceSeries?> HandleAsync(
       GetHistoricalPricesQuery query,
       CancellationToken cancellationToken = default)
    {
        // Validate input
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(query.Stock, nameof(query.Stock));
        if (!Enum.IsDefined(query.Interval))
        {
            throw new ArgumentOutOfRangeException(nameof(query.Interval));
        }
        if (!Enum.IsDefined(query.Adjustment))
        {
            throw new ArgumentOutOfRangeException(nameof(query.Adjustment));
        }

        // Normalize timestamps
        var fromUtc = query.FromUtc.ToUniversalTime();
        var toUtc = query.ToUtc.ToUniversalTime();
        if (fromUtc >= toUtc)
        {
            throw new ArgumentException(
                "The starting date must be before the ending date.");
        }
        // Normalize the query to ensure that the provider receives UTC timestamps.
        var normalizedQuery = query with
        {
            FromUtc = fromUtc,
            ToUtc = toUtc
        };

        var result = await _historicalPriceProvider
            .GetHistoricalPricesAsync(normalizedQuery, cancellationToken);

        if (result is null) return null;

        // Normalize ordering for the charting layer.
        return result with
        {
            Bars = result.Bars
                .OrderBy(x => x.TimestampUtc)
                .ToArray()
        };
    }
}
