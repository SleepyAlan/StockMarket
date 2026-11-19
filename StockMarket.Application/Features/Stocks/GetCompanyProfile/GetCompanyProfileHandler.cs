using StockMarket.Application.Abstractions.StockMarketData;

namespace StockMarket.Application.Features.Stocks.GetCompanyProfile;

public sealed class GetCompanyProfileHandler
{
    private readonly ICompanyProfilerProvider _companyProfilerProvider;

	public GetCompanyProfileHandler(ICompanyProfilerProvider companyProfilerProvider)
	{
        _companyProfilerProvider = companyProfilerProvider;
	}

    public async Task<CompanyProfileResult?> HandleAsync(
        GetCompanyProfileQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(query.Stock, nameof(query.Stock));

        return await _companyProfilerProvider.GetCompanyProfileAsync(query.Stock, cancellationToken);
    }
}
