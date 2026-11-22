using System.Text.Json.Serialization;

namespace StockMarket.Infrastructure.StockMarketData.Finnhub.DTOs;

/// <summary>
/// Finnhub Symbol Lookup. Update this DTO if structure changes.
/// https://finnhub.io/docs/api/symbol-search
/// </summary>
public sealed class FinnhubSearchResponse
{
    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("result")]
    public List<FinnhubSymbolDto> Result { get; init; } = [];
}

public sealed class FinnhubSymbolDto
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("displaySymbol")]
    public string? DisplaySymbol { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }
}