using StockMarket.Application.Abstractions.Identity;
using StockMarket.Application.Abstractions.Persistence;

namespace StockMarket.Application.Features.Watchlists.GetWatchlists;

public sealed class GetWatchlistsHandler
{
    private readonly IWatchlistQueries _watchlistQueries;
    private readonly ICurrentUserService _currentUser;

    public GetWatchlistsHandler(IWatchlistQueries watchlistQueries, ICurrentUserService currentUser)
    {
        _watchlistQueries = watchlistQueries;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<WatchlistSummary>> HandleAsync(
        GetWatchlistsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Guid userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        return await _watchlistQueries
            .GetByUserIdAsync(userId, cancellationToken);
    }
}
