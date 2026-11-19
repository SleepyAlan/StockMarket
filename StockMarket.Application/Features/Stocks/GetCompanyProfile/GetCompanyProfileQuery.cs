using StockMarket.Application.Common.Models;

namespace StockMarket.Application.Features.Stocks.GetCompanyProfile;

public sealed record GetCompanyProfileQuery(StockReference Stock);
