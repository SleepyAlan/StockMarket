using Microsoft.Extensions.DependencyInjection;
using StockMarket.Application.Features.Stocks.GetStockQuote;
using StockMarket.Application.Features.Stocks.SearchStocks;
using StockMarket.Application.Features.Watchlists.AddStockToWatchlist;
using StockMarket.Application.Features.Watchlists.CreateWatchlist;

namespace StockMarket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SearchStocksHandler>();
        services.AddScoped<GetStockQuoteHandler>();

        services.AddScoped<CreateWatchlistHandler>();
        services.AddScoped<AddStockToWatchlistHandler>();
        return services;
    }
}
