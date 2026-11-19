using StockMarket.Application.Abstractions.StockMarketData;

namespace StockMarket.Application.Features.Stocks.GetStockQuote;

public sealed class GetStockQuoteHandler
{
    private readonly IQuoteProvider _quoteProvider;

    public GetStockQuoteHandler(IQuoteProvider quoteProvider)
    {
        _quoteProvider = quoteProvider;
    }

    public async Task<StockQuote?> HandleAsync(
        GetStockQuoteQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(query.Stock, nameof(query.Stock));

        return await _quoteProvider.GetStockQuoteAsync(query.Stock, cancellationToken);
    }
}
