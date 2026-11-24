using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Infrastructure.Caching;
using StockMarket.Infrastructure.Persistence;
using StockMarket.Infrastructure.Persistence.Queries;
using StockMarket.Infrastructure.Persistence.Repositories;
using StockMarket.Infrastructure.StockMarketData.Alpaca;
using StockMarket.Infrastructure.StockMarketData.Alpaca.Providers;
using StockMarket.Infrastructure.StockMarketData.Finnhub;
using StockMarket.Infrastructure.StockMarketData.Finnhub.Providers;

namespace StockMarket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // SQL Server / EF Core
        services.AddDbContext<ApplicationDbContext>(options => // Defaults to Scoped
        {
            options.UseSqlServer(
                configuration.GetConnectionString(
                    "DefaultConnection"));
        });

        // Persistence
        services.AddScoped<IUnitOfWork>(
            sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IStockCatalogRepository, StockCatalogRepository>();
        services.AddScoped<IWatchlistRepository, WatchlistRepository>();
        services.AddScoped<IWatchlistQueries, WatchlistQueries>();
        services.AddScoped<IStockListingReader, StockListingReader>();
        services.AddScoped<IListingCurrencyReader, ListingCurrencyReader>();

        // Caching
        services.AddMemoryCache();


        //// Finnhub
        // Configuration
        services.AddOptions<FinnhubOptions>()
            .Bind(configuration.GetSection(FinnhubOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.ApiKey), "Finnhub API key is required.")
            .ValidateOnStart();
        // HTTP client
        services.AddHttpClient<FinnhubClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<FinnhubOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("X-Finnhub-Token", options.ApiKey);
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 2;
        });
        // Provider implementations
        services.AddScoped<IStockSearchProvider, FinnhubStockSearchProvider>();
        services.AddScoped<ICompanyProfileProvider, FinnhubCompanyProfileProvider>();
        services.AddScoped<IQuoteProvider, FinnhubQuoteProvider>();
        services.AddScoped<IStockListingMetadataProvider, FinnhubStockListingMetadataProvider>();

        //TODO: Uncomment after services above are tested and working properly
        //services.Decorate<IStockSearchProvider, CachedStockSearchProvider>();
        //services.Decorate<ICompanyProfileProvider, CachedCompanyProfileProvider>();
        //services.Decorate<IQuoteProvider, CachedQuoteProvider>();


        //// Alpaca
        // Configuration
        services.AddOptions<AlpacaOptions>()
            .Bind(configuration.GetSection(AlpacaOptions.SectionName))
            .Validate(x =>
                !string.IsNullOrWhiteSpace(x.ApiKey) &&
                !string.IsNullOrWhiteSpace(x.ApiSecret),
                "Alpaca credentials are required.")
            .ValidateOnStart();
        // HTTP client
        services.AddHttpClient<AlpacaClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<AlpacaOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("APCA-API-KEY-ID", options.ApiKey);
            client.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", options.ApiSecret);
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 2;
        });
        // Provider implementations
        services.AddScoped<IHistoricalPriceProvider, AlpacaHistoricalPriceProvider>();


        return services;
    }
}
