using FluentAssertions;
using Moq;
using StockMarket.Application.Abstractions.Identity;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Application.Features.Watchlists.AddStockToWatchlist;
using StockMarket.Domain.Entities;

namespace StockMarket.Application.Tests.WatchlistTests;

public class AddStockToWatchlistHandlerTests
{
    private readonly Mock<IWatchlistRepository> _repositoryMock;
    private readonly Mock<IStockListingReader> _stockListingReaderMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddStockToWatchlistHandler _sut;

    public AddStockToWatchlistHandlerTests()
    {
        _repositoryMock = new Mock<IWatchlistRepository>();
        _stockListingReaderMock = new Mock<IStockListingReader>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new AddStockToWatchlistHandler(
            _repositoryMock.Object,
            _currentUserMock.Object,
            _unitOfWorkMock.Object);
    }

    #region HandleAsync Tests

    [Fact]
    public async Task HandleAsync_WhenCommandIsNull_ThrowsArgumentNullException()
    {
        // Act
        var action = () => _sut.HandleAsync(null!, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("command");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task HandleAsync_WhenIdIsEmpty_ThrowsArgumentException(bool watchlistIdEmpty, bool stockListingIdEmpty)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var watchlistId = watchlistIdEmpty ? Guid.Empty : Guid.NewGuid();
        var stockListingId = stockListingIdEmpty ? Guid.Empty : Guid.NewGuid();
        var command = new AddStockToWatchlistCommand(watchlistId, stockListingId);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        // Act
        var action = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var watchlistId = Guid.NewGuid();
        var stockListingId = Guid.NewGuid();
        var command = new AddStockToWatchlistCommand(watchlistId, stockListingId);
        _currentUserMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act
        var action = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task HandleAsync_WhenWatchlistNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();
        var stockListingId = Guid.NewGuid();
        var command = new AddStockToWatchlistCommand(watchlistId, stockListingId);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.GetWatchlistByIdAsync(watchlistId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Watchlist?)null);

        // Act
        var action = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenWatchlistBelongsToDifferentUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();
        var stockListingId = Guid.NewGuid();
        var command = new AddStockToWatchlistCommand(watchlistId, stockListingId);

        var watchlist = new Watchlist(differentUserId, "Test Watchlist");

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.GetWatchlistByIdAsync(watchlistId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchlist);

        // Act
        var action = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenStockIsSuccessfullyAdded_SavesChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();
        var stockListingId = Guid.NewGuid();
        var command = new AddStockToWatchlistCommand(watchlistId, stockListingId);

        var watchlist = new Watchlist(userId, "Test Watchlist");

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.GetWatchlistByIdAsync(watchlistId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchlist);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        watchlist.Items.Should().HaveCount(1);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAddingDuplicateStock_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();
        var stockListingId = Guid.NewGuid();

        var watchlist = new Watchlist(userId, "Test Watchlist");
        watchlist.AddStock(stockListingId); // Add stock first time

        var command = new AddStockToWatchlistCommand(watchlistId, stockListingId);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.GetWatchlistByIdAsync(watchlistId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchlist);

        // Act
        var action = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationTokenIsCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();
        var stockListingId = Guid.NewGuid();
        var command = new AddStockToWatchlistCommand(watchlistId, stockListingId);
        var cancellationToken = new CancellationToken(canceled: true);

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.GetWatchlistByIdAsync(watchlistId, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var action = () => _sut.HandleAsync(command, cancellationToken);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(IOException))]
    [InlineData(typeof(TimeoutException))]
    public async Task HandleAsync_WhenRepositoryThrowsException_PropagatesException(Type exceptionType)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();
        var stockListingId = Guid.NewGuid();
        var command = new AddStockToWatchlistCommand(watchlistId, stockListingId);
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.GetWatchlistByIdAsync(watchlistId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var action = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .Where(ex => ex.GetType() == exceptionType);
    }

    [Theory]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(IOException))]
    [InlineData(typeof(TimeoutException))]
    public async Task HandleAsync_WhenUnitOfWorkThrowsException_PropagatesException(Type exceptionType)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();
        var stockListingId = Guid.NewGuid();
        var command = new AddStockToWatchlistCommand(watchlistId, stockListingId);
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        var watchlist = new Watchlist(userId, "Test Watchlist");

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.GetWatchlistByIdAsync(watchlistId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchlist);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var action = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .Where(ex => ex.GetType() == exceptionType);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();
        var stockListingId = Guid.NewGuid();
        var command = new AddStockToWatchlistCommand(watchlistId, stockListingId);
        var cancellationToken = new CancellationToken();

        var watchlist = new Watchlist(userId, "Test Watchlist");

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.GetWatchlistByIdAsync(watchlistId, cancellationToken))
            .ReturnsAsync(watchlist);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        _repositoryMock.Verify(
            x => x.GetWatchlistByIdAsync(watchlistId, cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToUnitOfWork()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();
        var stockListingId = Guid.NewGuid();
        var command = new AddStockToWatchlistCommand(watchlistId, stockListingId);
        var cancellationToken = new CancellationToken();

        var watchlist = new Watchlist(userId, "Test Watchlist");

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.GetWatchlistByIdAsync(watchlistId, cancellationToken))
            .ReturnsAsync(watchlist);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    #endregion
}
