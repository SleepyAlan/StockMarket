using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetCompanyProfile;

namespace StockMarket.Application.Abstractions.StockMarketData;

public interface ICompanyProfileProvider
{
    /// <summary>
    /// Asynchronously gets the company profile for the specified stock reference.
    /// </summary>
    /// <param name="stockReference">The stock identifier used to locate the company profile.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the company profile when available;
    /// otherwise, <see langword="null"/>.</returns>
    Task<CompanyProfileResult?> GetCompanyProfileAsync(
        StockReference stockReference, CancellationToken cancellationToken = default);
}
