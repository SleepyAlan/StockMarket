using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetCompanyProfile;
using StockMarket.Application.Features.Stocks.GetStockQuote;

namespace StockMarket.Application.Features.Stocks.GetStockDetails;

/// <summary>
/// Represents detailed stock information, including reference data, company profile, and current quote.
/// </summary>
/// <remarks>Profile and quote data can be null when the information is unavailable.</remarks>
/// <param name="Stock">The stock reference information.</param>
/// <param name="Profile">The company profile information, if available.</param>
/// <param name="Quote">The latest stock quote, if available.</param>
public sealed record StockDetailsResult(
    StockReference Stock,
    CompanyProfileResult? Profile,
    StockQuote? Quote);
