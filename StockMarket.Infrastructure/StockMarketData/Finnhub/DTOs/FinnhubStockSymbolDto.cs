using System.Text.Json.Serialization;

namespace StockMarket.Infrastructure.StockMarketData.Finnhub.DTOs;

public sealed class FinnhubStockSymbolDto
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("displaySymbol")]
    public string? DisplaySymbol { get; init; }

    [JsonPropertyName("mic")]
    public string? Mic { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("figi")]
    public string? Figi { get; init; }

    [JsonPropertyName("shareClassFIGI")]
    public string? ShareClassFigi { get; init; }
}
