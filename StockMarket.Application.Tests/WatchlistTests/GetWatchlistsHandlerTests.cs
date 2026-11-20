using FluentAssertions;
using Moq;
using StockMarket.Application.Abstractions.Identity;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Application.Features.Watchlists.GetWatchlists;
using Xunit;

namespace StockMarket.Application.Tests.WatchlistTests;

public class GetWatchlistsHandlerTests
{
    private readonly Mock<IWatchlistQueries> _watchlistQueriesMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly GetWatchlistsHandler _handler;

    public GetWatchlistsHandlerTests()
    {
        _watchlistQueriesMock = new Mock<IWatchlistQueries>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _handler = new GetWatchlistsHandler(_watchlistQueriesMock.Object, _currentUserMock.Object);
    }

    #region HandleAsync Tests

    [Fact]
    public async Task HandleAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _handler.HandleAsync(null!, CancellationToken.None));
        exception.ParamName.Should().Be("query");
    }

    [Fact]
    public async Task HandleAsync_WithUnauthenticatedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _currentUserMock.Setup(x => x.UserId).Returns((Guid?)null);
        var query = new GetWatchlistsQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
        exception.Message.Should().Be("User must be authenticated.");

        _watchlistQueriesMock.Verify(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithAuthenticatedUserAndEmptyWatchlists_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emptyWatchlists = new List<WatchlistSummary>();

        _watchlistQueriesMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyWatchlists);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var query = new GetWatchlistsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeEmpty();
        _watchlistQueriesMock.Verify(x => x.GetByUserIdAsync(userId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public async Task HandleAsync_WithAuthenticatedUserAndWatchlists_ReturnsCorrectWatchlistCount(int watchlistCount)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var watchlists = Enumerable.Range(1, watchlistCount)
            .Select(i => new WatchlistSummary(
                Id: Guid.NewGuid(),
                Name: $"Watchlist {i}",
                StockCount: i * 5,
                CreatedAtUtc: DateTimeOffset.UtcNow.AddDays(-i)))
            .ToList();

        _watchlistQueriesMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchlists);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var query = new GetWatchlistsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(watchlistCount);
        result.Should().BeEquivalentTo(watchlists);
        _watchlistQueriesMock.Verify(x => x.GetByUserIdAsync(userId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_PassesCorrectUserIdToQueries()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var watchlists = new List<WatchlistSummary>
        {
            new(Guid.NewGuid(), "Tech Stocks", 15, DateTimeOffset.UtcNow)
        };

        _watchlistQueriesMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchlists);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var query = new GetWatchlistsQuery();

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _watchlistQueriesMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_CancellationTokenPropagates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _watchlistQueriesMock
            .Setup(x => x.GetByUserIdAsync(userId, cancellationToken))
            .ReturnsAsync(new List<WatchlistSummary>());

        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var query = new GetWatchlistsQuery();

        // Act
        await _handler.HandleAsync(query, cancellationToken);

        // Assert
        _watchlistQueriesMock.Verify(
            x => x.GetByUserIdAsync(userId, cancellationToken),
            Times.Once);
    }

    #endregion
}
