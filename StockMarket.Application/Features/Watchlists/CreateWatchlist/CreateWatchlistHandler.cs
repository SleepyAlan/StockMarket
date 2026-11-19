using StockMarket.Application.Abstractions.Identity;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Domain.Entities;

namespace StockMarket.Application.Features.Watchlists.CreateWatchlist;

public sealed class CreateWatchlistHandler
{
    private readonly IWatchlistRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateWatchlistHandler(
        IWatchlistRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> HandleAsync(
        CreateWatchlistCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        Guid userId = _currentUser.UserId ??
            throw new UnauthorizedAccessException("User is not authenticated.");

        var watchlist = new Watchlist(userId, command.Name);

        bool exists = await _repository
            .WatchlistExistsByNameAsync(userId, command.Name, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException(
                $"A watchlist with the name '{command.Name}' already exists.");
        }

        await _repository.AddAsync(watchlist, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return watchlist.WatchlistId;
    }
}
