namespace StockMarket.Infrastructure.StockMarketData.Alpaca;

public sealed class AlpacaOptions
{
    public const string SectionName = "Alpaca";
    public string BaseUrl { get; set; } = "https://data.alpaca.markets/v2/";
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}
