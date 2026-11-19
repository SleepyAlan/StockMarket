using StockMarket.Application.Abstractions.Identity;
using StockMarket.Application.Abstractions.Persistence;

namespace StockMarket.Application.Features.Watchlists.AddStockToWatchlist;

public sealed class AddStockToWatchlistHandler
{
    IWatchlistRepository _repository;
    ICurrentUserService _currentUser;
    IUnitOfWork _unitOfWork;

    public AddStockToWatchlistHandler(
        IWatchlistRepository repository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        AddStockToWatchlistCommand command, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        if (command.WatchlistId == Guid.Empty || command.StockListingId == Guid.Empty)
        {
            throw new ArgumentException("Watchlist and stock listing IDs are required.");
        }

        Guid userId = _currentUser.UserId ?? 
            throw new UnauthorizedAccessException("User must be authenticated.");

        var watchlist = await _repository
            .GetWatchlistByIdAsync(command.WatchlistId, cancellationToken);

        if (watchlist is null || watchlist.UserId != userId)
        {
            throw new KeyNotFoundException("Watchlist not found.");
        }

        watchlist.AddStock(command.StockListingId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
