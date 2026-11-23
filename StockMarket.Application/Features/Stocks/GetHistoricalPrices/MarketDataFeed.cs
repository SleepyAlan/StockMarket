namespace StockMarket.Application.Features.Stocks.GetHistoricalPrices;

public enum MarketDataFeed
{
    Iex,
    Sip
}

public static class MarketDataFeedExtensions
{
    public static string ToFriendlyString(this MarketDataFeed feed) => feed switch
    {
        MarketDataFeed.Iex => "iex",
        MarketDataFeed.Sip => "sip",
        _ => throw new ArgumentOutOfRangeException(nameof(feed), feed, null)
    };
}