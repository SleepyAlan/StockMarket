using FluentAssertions;
using StockMarket.Domain.Entities;

namespace StockMarket.Domain.Tests.EntitiesTests;

public class WatchListTests
{
    #region Constructor Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_GivenInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var userId = Guid.NewGuid();
        // Act
        Action act = () => new Watchlist(userId, invalidName!);
        // Assert
        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Constructor_GivenEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyUserId = Guid.Empty;
        var name = "My WatchList";
        // Act
        Action act = () => new Watchlist(emptyUserId, name);
        // Assert
        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("userId");
    }

    [Fact]
    public void Constructor_GivenValidParameters_ShouldCreateWatchList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = "My WatchList";
        // Act
        var watchList = new Watchlist(userId, name);
        // Assert
        watchList.UserId.Should().Be(userId);
        watchList.Name.Should().Be(name);
        watchList.Items.Should().BeEmpty();
        watchList.WatchlistId.Should().NotBeEmpty();
        watchList.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
    }

    #endregion

    #region AddStock Tests

    [Fact]
    public void AddStock_GivenValidStockListingId_ShouldAddStockToWatchList()
    {
        // Arrange
        var watchList = new Watchlist(Guid.NewGuid(), "My WatchList");
        var stockListingId = Guid.NewGuid();
        // Act
        watchList.AddStock(stockListingId);
        // Assert
        watchList.Items.Should().ContainSingle(item => item.StockListingId == stockListingId);
    }

    [Fact]
    public void AddStock_GivenDuplicateStockListingId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var watchList = new Watchlist(Guid.NewGuid(), "My WatchList");
        var stockListingId = Guid.NewGuid();
        watchList.AddStock(stockListingId);
        // Act
        Action act = () => watchList.AddStock(stockListingId);
        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddStock_GivenEmptyStockListingId_ShouldThrowArgumentException()
    {
        // Arrange
        var watchList = new Watchlist(Guid.NewGuid(), "My WatchList");
        var emptyStockListingId = Guid.Empty;
        // Act
        Action act = () => watchList.AddStock(emptyStockListingId);
        // Assert
        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("stockListingId");
    }

    #endregion

    #region RemoveStock Tests

    [Fact]
    public void RemoveStock_GivenExistingStockListingId_ShouldRemoveStockFromWatchList()
    {
        // Arrange
        var watchList = new Watchlist(Guid.NewGuid(), "My WatchList");
        var stockListingId = Guid.NewGuid();
        watchList.AddStock(stockListingId);
        // Act
        watchList.RemoveStock(stockListingId);
        // Assert
        watchList.Items.Should().NotContain(item => item.StockListingId == stockListingId);
    }

    [Fact]
    public void RemoveStock_GivenNonExistingStockListingId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var watchList = new Watchlist(Guid.NewGuid(), "My WatchList");
        var nonExistingStockListingId = Guid.NewGuid();
        // Act
        Action act = () => watchList.RemoveStock(nonExistingStockListingId);
        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveStock_GivenEmptyStockListingId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var watchList = new Watchlist(Guid.NewGuid(), "My WatchList");
        var emptyStockListingId = Guid.Empty;
        // Act
        Action act = () => watchList.RemoveStock(emptyStockListingId);
        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region RenameStock Tests

    [Fact]
    public void Rename_GivenValidNewName_ShouldRenameWatchList()
    {
        // Arrange
        var watchList = new Watchlist(Guid.NewGuid(), "My WatchList");
        var newName = "Renamed WatchList";
        // Act
        watchList.Rename(newName);
        // Assert
        watchList.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Rename_GivenInvalidNewName_ShouldThrowArgumentException(string? invalidNewName)
    {
        // Arrange
        var watchList = new Watchlist(Guid.NewGuid(), "My WatchList");
        // Act
        Action act = () => watchList.Rename(invalidNewName!);
        // Assert
        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("newName");
    }

    #endregion
}
