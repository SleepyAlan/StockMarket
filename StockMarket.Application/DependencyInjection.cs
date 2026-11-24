using Microsoft.Extensions.DependencyInjection;
using StockMarket.Application.Features.Stocks.GetCompanyProfile;
using StockMarket.Application.Features.Stocks.GetHistoricalPrices;
using StockMarket.Application.Features.Stocks.GetStockDetails;
using StockMarket.Application.Features.Stocks.GetStockQuote;
using StockMarket.Application.Features.Stocks.ResolveOrRegisterStockListing;
using StockMarket.Application.Features.Stocks.SearchStocks;
using StockMarket.Application.Features.Watchlists.AddStockToWatchlist;
using StockMarket.Application.Features.Watchlists.CreateWatchlist;
using StockMarket.Application.Features.Watchlists.GetWatchlists;
using StockMarket.Application.Features.Watchlists.RemoveStockFromWatchlist;

namespace StockMarket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services // Queries
            .AddScoped<SearchStocksHandler>()
            .AddScoped<GetStockQuoteHandler>()
            .AddScoped<GetCompanyProfileHandler>()
            .AddScoped<GetHistoricalPricesHandler>()
            .AddScoped<GetStockDetailsHandler>()
            .AddScoped<GetWatchlistsHandler>();
     
        services // Commands
            .AddScoped<CreateWatchlistHandler>()
            .AddScoped<AddStockToWatchlistHandler>()
            .AddScoped<RemoveStockFromWatchlistHandler>();
        services.AddScoped<ResolveOrRegisterStockListingHandler>();

        return services;
    }
}
