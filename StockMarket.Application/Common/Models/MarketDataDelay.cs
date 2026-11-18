namespace StockMarket.Application.Common.Models;

public enum MarketDataDelay
{
    Unknown = 0,
    RealTime = 1,
    Delayed = 2,
    EndOfDay = 3
}

public static class MarketDataDelayExtensions
{
    public static string ToFriendlyString(this MarketDataDelay delay) =>
        delay switch
        {
            MarketDataDelay.Unknown => "Unknown",
            MarketDataDelay.RealTime => "Real-time",
            MarketDataDelay.Delayed => "Delayed",
            MarketDataDelay.EndOfDay => "End of day",
            _ => throw new ArgumentOutOfRangeException(nameof(delay), delay, null)
        };
}