using System.Text.Json.Serialization;

namespace StockMarket.Infrastructure.StockMarketData.Alpaca.DTOs;

public sealed class AlpacaHistoricalBarsResponse
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("bars")]
    public List<AlpacaBarDto>? Bars { get; init; }

    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; init; }
}

public sealed class AlpacaBarDto
{
    [JsonPropertyName("t")]
    public DateTimeOffset Timestamp { get; init; }

    [JsonPropertyName("o")]
    public decimal Open { get; init; }

    [JsonPropertyName("h")]
    public decimal High { get; init; }

    [JsonPropertyName("l")]
    public decimal Low { get; init; }

    [JsonPropertyName("c")]
    public decimal Close { get; init; }

    [JsonPropertyName("v")]
    public long Volume { get; init; }
}
