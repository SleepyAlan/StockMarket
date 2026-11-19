using StockMarket.Application.Abstractions.Identity;
using StockMarket.Application.Abstractions.Persistence;

namespace StockMarket.Application.Features.Watchlists.RemoveStockFromWatchlist;

public sealed class RemoveStockFromWatchlistHandler
{
    private readonly IWatchlistRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveStockFromWatchlistHandler(
        IWatchlistRepository repository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUser = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(RemoveStockFromWatchlistCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        if (command.WatchlistId == Guid.Empty || command.StockListingId == Guid.Empty)
        {
            throw new ArgumentException("Watchlist and stock listing IDs are required.");
        }

        Guid userId = _currentUser.UserId ??
            throw new UnauthorizedAccessException("User must be authenticated.");

        var watchlist = await _repository.GetWatchlistByIdAsync(command.WatchlistId, cancellationToken);

        if (watchlist is null || watchlist.UserId != userId)
        {
            throw new KeyNotFoundException("Watchlist not found.");
        }
            
        watchlist.RemoveStock(command.StockListingId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
