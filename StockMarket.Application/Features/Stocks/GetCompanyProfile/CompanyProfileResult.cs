using StockMarket.Application.Common.Models;

namespace StockMarket.Application.Features.Stocks.GetCompanyProfile;

public sealed record CompanyProfileResult(
    StockReference Stock,
    string Name,
    string? CountryCode,
    string? Industry,
    string? Sector,
    string? Description,
    string? WebsiteUrl);
