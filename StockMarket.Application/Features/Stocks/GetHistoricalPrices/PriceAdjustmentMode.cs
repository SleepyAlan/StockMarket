namespace StockMarket.Application.Features.Stocks.GetHistoricalPrices;

public enum PriceAdjustmentMode
{
    Raw,
    SplitAdjusted,
    DividendAdjusted,
    SpinOffAdjusted,
    SplitDividendAndSpinOffAdjusted
}
