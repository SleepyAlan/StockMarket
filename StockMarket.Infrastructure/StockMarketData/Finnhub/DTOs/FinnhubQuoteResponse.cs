using System.Text.Json.Serialization;

namespace StockMarket.Infrastructure.StockMarketData.Finnhub.DTOs;

public sealed class FinnhubQuoteResponse
{
    [JsonPropertyName("c")]
    public decimal? Current { get; init; }

    [JsonPropertyName("pc")]
    public decimal? PreviousClose { get; init; }

    [JsonPropertyName("t")]
    public long? Timestamp { get; init; }
}
