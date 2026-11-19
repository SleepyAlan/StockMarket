namespace StockMarket.Application.Abstractions.Persistence;

public interface IStockListingReader
{
    Task<bool> StockListingExistsByIdAsync(
        Guid stockListingId, CancellationToken cancellationToken = default);
}
