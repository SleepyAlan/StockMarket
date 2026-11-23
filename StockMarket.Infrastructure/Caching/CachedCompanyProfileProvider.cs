using Microsoft.Extensions.Caching.Memory;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetCompanyProfile;

namespace StockMarket.Infrastructure.Caching;

public sealed class CachedCompanyProfileProvider : ICompanyProfileProvider
{
    private readonly ICompanyProfileProvider _companyProfileProvider;
    private readonly IMemoryCache _cache;

    public CachedCompanyProfileProvider(
        ICompanyProfileProvider companyProfileProvider,
        IMemoryCache cache)
    {
        _companyProfileProvider = companyProfileProvider;
        _cache = cache;
    }

    public async Task<CompanyProfileResult?> GetCompanyProfileAsync(
        StockReference stockReference, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"stocks:company-profile:{stockReference.Symbol}:{stockReference.Exchange}";

        if (_cache.TryGetValue(cacheKey, out CompanyProfileResult? cached) && cached is not null)
        {
            return cached;
        }

        var profile = await _companyProfileProvider.GetCompanyProfileAsync(stockReference, cancellationToken);
        if (profile is not null)
        {
            _cache.Set(cacheKey, profile, TimeSpan.FromHours(12));
        }
        return profile;
    }
}
