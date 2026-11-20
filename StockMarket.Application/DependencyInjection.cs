using Microsoft.Extensions.DependencyInjection;
using StockMarket.Application.Features.Stocks.GetCompanyProfile;
using StockMarket.Application.Features.Stocks.GetStockQuote;
using StockMarket.Application.Features.Stocks.SearchStocks;
using StockMarket.Application.Features.Watchlists.AddStockToWatchlist;
using StockMarket.Application.Features.Watchlists.CreateWatchlist;
using StockMarket.Application.Features.Watchlists.RemoveStockFromWatchlist;

namespace StockMarket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<SearchStocksHandler>();
        services.AddScoped<GetStockQuoteHandler>();
        services.AddScoped<GetCompanyProfileHandler>();

        services.AddScoped<CreateWatchlistHandler>();
        services.AddScoped<AddStockToWatchlistHandler>();
        services.AddScoped<RemoveStockFromWatchlistHandler>();
        return services;
    }
}
