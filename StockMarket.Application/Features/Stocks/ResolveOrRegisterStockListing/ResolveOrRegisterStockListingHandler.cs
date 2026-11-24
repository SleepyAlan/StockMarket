using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Domain.Entities;

namespace StockMarket.Application.Features.Stocks.ResolveOrRegisterStockListing;

public sealed class ResolveOrRegisterStockListingHandler
{
    private readonly IStockCatalogRepository _repository;
    private readonly IStockListingMetadataProvider _stockListingProvider;
    private readonly ICompanyProfileProvider _companyProfileProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ResolveOrRegisterStockListingHandler(
        IStockCatalogRepository repository,
        IStockListingMetadataProvider stockListingProvider,
        ICompanyProfileProvider companyProfileProvider,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _stockListingProvider = stockListingProvider;
        _companyProfileProvider = companyProfileProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(
        ResolveOrRegisterStockListingCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Stock);
        ArgumentNullException.ThrowIfNull(command.Stock.Symbol);
        ArgumentNullException.ThrowIfNull(command.Stock.Exchange);

        var stock = command.Stock;

        var existingId = await _repository
            .GetListingIdAsync(stock, cancellationToken);

        if (existingId.HasValue)
        {
            return existingId.Value;
        }

        // Resolve the stock listing via the external provider.
        var verified = await _stockListingProvider
            .ResolveAsync(stock, cancellationToken)
            ?? throw new KeyNotFoundException("Stock listing could not be verified.");

        if (verified.Stock != stock)
        {
            throw new InvalidOperationException(
                "The provider returned a different stock listing.");
        }


        var profile = await _companyProfileProvider
            .GetCompanyProfileAsync(stock, cancellationToken);

        if (profile is null ||
            profile.Stock != stock ||
            string.IsNullOrWhiteSpace(profile.Name) ||
            string.IsNullOrWhiteSpace(profile.CountryCode))
        {
            throw new InvalidOperationException(
                "Verified company information is unavailable.");
        }

        // Reduces concurrent inserts, but does not eliminate the possibility
        // of a race condition. Ensure in repository/database enforces uniqueness.
        existingId = await _repository
            .GetListingIdAsync(stock, cancellationToken);

        if (existingId.HasValue)
        { 
            return existingId.Value;
        }

        var company = new Company(
            profile.Name,
            profile.CountryCode);

        company.UpdateCompanyProfile(
            profile.Name,
            profile.Sector,
            profile.Industry,
            profile.Description,
            profile.WebsiteUrl);

        var listing = new StockListing(
            company.CompanyId,
            stock.Symbol,
            stock.Exchange,
            verified.Currency);

        // Initiate the registration of both entities to the repository.
        await _repository.AddCompanyAsync(company, cancellationToken);
        await _repository.AddStockListingAsync(listing, cancellationToken);
        // Persist both entities atomically.
        await _unitOfWork.SaveChangesAsync(cancellationToken);


        return listing.StockListingId;
    }
}
