using FluentAssertions;
using Moq;
using StockMarket.Application.Abstractions.Identity;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Application.Features.Watchlists.CreateWatchlist;
using StockMarket.Domain.Entities;

namespace StockMarket.Application.Tests.WatchlistTests;

public class CreateWatchlistHandlerTests
{
    private readonly Mock<IWatchlistRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly CreateWatchlistHandler _sut;

    public CreateWatchlistHandlerTests()
    {
        _repositoryMock = new Mock<IWatchlistRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _sut = new CreateWatchlistHandler(
            _repositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _currentUserMock.Object);
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

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateWatchlistCommand("My Watchlist");
        _currentUserMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act
        var action = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task HandleAsync_WhenWatchlistNameAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateWatchlistCommand("My Watchlist");
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.WatchlistExistsByNameAsync(userId, command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var action = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenWatchlistCreationSucceeds_ReturnsValidWatchlistId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateWatchlistCommand("My Watchlist");
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.WatchlistExistsByNameAsync(userId, command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _repositoryMock.Verify(
            x => x.WatchlistExistsByNameAsync(userId, command.Name, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Watchlist>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationTokenIsCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateWatchlistCommand("My Watchlist");
        var cancellationToken = new CancellationToken(canceled: true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.WatchlistExistsByNameAsync(userId, command.Name, cancellationToken))
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
        var command = new CreateWatchlistCommand("My Watchlist");
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.WatchlistExistsByNameAsync(userId, command.Name, It.IsAny<CancellationToken>()))
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
        var command = new CreateWatchlistCommand("My Watchlist");
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.WatchlistExistsByNameAsync(userId, command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
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
        var command = new CreateWatchlistCommand("My Watchlist");
        var cancellationToken = new CancellationToken();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.WatchlistExistsByNameAsync(userId, command.Name, cancellationToken))
            .ReturnsAsync(false);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        _repositoryMock.Verify(
            x => x.WatchlistExistsByNameAsync(userId, command.Name, cancellationToken),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToUnitOfWork()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateWatchlistCommand("My Watchlist");
        var cancellationToken = new CancellationToken();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _repositoryMock
            .Setup(x => x.WatchlistExistsByNameAsync(userId, command.Name, cancellationToken))
            .ReturnsAsync(false);
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
