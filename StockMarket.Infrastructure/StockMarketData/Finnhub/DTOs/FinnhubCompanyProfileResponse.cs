using System.Text.Json.Serialization;

namespace StockMarket.Infrastructure.StockMarketData.Finnhub.DTOs;

public sealed class FinnhubCompanyProfileResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("country")]
    public string? Country { get; init; }

    [JsonPropertyName("ticker")]
    public string? Ticker { get; init; }

    [JsonPropertyName("finnhubIndustry")]
    public string? Industry { get; init; }

    [JsonPropertyName("weburl")]
    public string? WebsiteUrl { get; init; }

    [JsonPropertyName("currency")]
    public string? ReportingCurrency { get; init; }
    // Possibility that trading currency is different from reporting currency.
}
