using StockMarket.Application.Abstractions.StockMarketData;

namespace StockMarket.Application.Features.Stocks.GetStockDetails;

public sealed class GetStockDetailsHandler
{
    private readonly ICompanyProfileProvider _companyProfileProvider;
    private readonly IQuoteProvider _quoteProvider;

    public GetStockDetailsHandler(ICompanyProfileProvider companyProfileProvider, IQuoteProvider quoteProvider)
    {
        _companyProfileProvider = companyProfileProvider;
        _quoteProvider = quoteProvider;
    }

    public async Task<StockDetailsResult?> HandleAsync(
        GetStockDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(query.Stock, nameof(query.Stock));

        // Both operations are independent.
        var profileTask = _companyProfileProvider
            .GetCompanyProfileAsync(query.Stock, cancellationToken);
        var quoteTask = _quoteProvider
            .GetStockQuoteAsync(query.Stock, cancellationToken);

        // Execute concurrently.
        await Task.WhenAll(profileTask, quoteTask);
        var profile = await profileTask;
        var quote = await quoteTask;

        // Neither provider returned data.
        if (profile is null && quote is null) return null;

        return new StockDetailsResult(
            Stock: query.Stock,
            Profile: profile,
            Quote: quote);
    }
}
