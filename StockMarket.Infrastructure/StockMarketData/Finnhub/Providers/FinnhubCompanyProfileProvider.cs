using Microsoft.Extensions.Logging;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetCompanyProfile;
using StockMarket.Infrastructure.StockMarketData.Finnhub.DTOs;

namespace StockMarket.Infrastructure.StockMarketData.Finnhub.Providers;

public sealed class FinnhubCompanyProfileProvider : ICompanyProfileProvider
{
    private readonly FinnhubClient _client;
    private readonly ILogger<FinnhubCompanyProfileProvider> _logger;

    public FinnhubCompanyProfileProvider(FinnhubClient client, ILogger<FinnhubCompanyProfileProvider> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<CompanyProfileResult?> GetCompanyProfileAsync(
        StockReference stockReference,
        CancellationToken cancellationToken = default)
    {
        var relativePath = $"stock/profile2?symbol={Uri.EscapeDataString(stockReference.Symbol.Value)}";
        
        var response = await _client.GetAsync<FinnhubCompanyProfileResponse>(relativePath, cancellationToken);

        if (response is null || 
            string.IsNullOrWhiteSpace(response.Name) ||
            !string.Equals(response.Ticker, stockReference.Symbol.Value, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("No company profile found for symbol: {Symbol}", stockReference.Symbol.Value);
            return null;
        }

        var companyProfileResult = new CompanyProfileResult(
            Stock: stockReference,
            Name: response.Name,
            CountryCode: response.Country,
            Industry: response.Industry,
            Sector: null,
            Description: null,
            WebsiteUrl: response.WebsiteUrl);

        return companyProfileResult;
    }
}
