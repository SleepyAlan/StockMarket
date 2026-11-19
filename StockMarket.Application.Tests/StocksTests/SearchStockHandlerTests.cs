using FluentAssertions;
using Moq;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Features.Stocks.SearchStocks;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Application.Tests.StocksTests;

public class SearchStockHandlerTests
{
    private readonly Mock<IStockSearchProvider> _stockSearchProviderMock;
    private readonly SearchStocksHandler _sut;

    public SearchStockHandlerTests()
    {
        _stockSearchProviderMock = new Mock<IStockSearchProvider>();
        _sut = new SearchStocksHandler(_stockSearchProviderMock.Object);
    }

    #region HandleAsync Tests

    [Fact]
    public async Task HandleAsync_WhenQueryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var action = () => _sut.HandleAsync(null!, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("query");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task HandleAsync_WhenSearchTermIsNullOrWhitespace_ThrowsArgumentException(string? searchTerm)
    {
        // Arrange
        var query = new SearchStocksQuery(searchTerm!);

        // Act
        var action = () => _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_WhenSearchTermIs255Characters_Succeeds()
    {
        // Arrange
        var searchTerm = new string('a', 255);
        var query = new SearchStocksQuery(searchTerm);
        var expectedResults = new List<StockSearchResult>().AsReadOnly();

        _stockSearchProviderMock
            .Setup(x => x.SearchStocksAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var action = () => _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_WhenSearchTermExceeds255Characters_ThrowsArgumentException()
    {
        // Arrange
        var longSearchTerm = new string('a', 256);
        var query = new SearchStocksQuery(longSearchTerm);

        // Act
        var action = () => _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("  AAPL", "AAPL")]
    [InlineData("AAPL  ", "AAPL")]
    [InlineData("  AAPL  ", "AAPL")]
    [InlineData("\tApple\t", "Apple")]
    [InlineData("   Microsoft   ", "Microsoft")]
    public async Task HandleAsync_WhenSearchTermHasLeadingOrTrailingWhitespace_TrimsAndProcesses(
        string searchTermWithWhitespace,
        string expectedTrimmedTerm)
    {
        // Arrange
        var query = new SearchStocksQuery(searchTermWithWhitespace);
        var expectedResults = new List<StockSearchResult>
        {
            new(
                new TickerSymbol("AAPL"),
                "Apple Inc.",
                new ExchangeCode("NSDA"),
                new CurrencyCode("USD")
            )
        }.AsReadOnly();

        _stockSearchProviderMock
            .Setup(x => x.SearchStocksAsync(expectedTrimmedTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull().And.HaveCount(1);
        _stockSearchProviderMock.Verify(
            x => x.SearchStocksAsync(expectedTrimmedTerm, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WhenProviderReturnsEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var query = new SearchStocksQuery("NONEXISTENT");
        var emptyResults = new List<StockSearchResult>().AsReadOnly();

        _stockSearchProviderMock
            .Setup(x => x.SearchStocksAsync("NONEXISTENT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResults);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenProviderReturnsResults_ReturnsResults()
    {
        // Arrange
        var query = new SearchStocksQuery("Apple");
        var expectedResults = new List<StockSearchResult>
        {
            new(
                new TickerSymbol("AAPL"),
                "Apple Inc.",
                new ExchangeCode("NSDA"),
                new CurrencyCode("USD")
            ),
            new(
                new TickerSymbol("APPL"),
                "Apple Hospitality REIT Inc.",
                new ExchangeCode("NYSE"),
                new CurrencyCode("USD")
            )
        }.AsReadOnly();

        _stockSearchProviderMock
            .Setup(x => x.SearchStocksAsync("Apple", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull().And.HaveCount(2);
        result.Should().BeEquivalentTo(expectedResults);
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationTokenIsCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var query = new SearchStocksQuery("Apple");
        var cancellationToken = new CancellationToken(canceled: true);

        _stockSearchProviderMock
            .Setup(x => x.SearchStocksAsync("Apple", cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var action = () => _sut.HandleAsync(query, cancellationToken);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(typeof(HttpRequestException))]
    [InlineData(typeof(TimeoutException))]
    [InlineData(typeof(InvalidOperationException))]
    public async Task HandleAsync_WhenProviderThrowsException_PropagatesException(Type exceptionType)
    {
        // Arrange
        var query = new SearchStocksQuery("Apple");
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        _stockSearchProviderMock
            .Setup(x => x.SearchStocksAsync("Apple", It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var action = () => _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .Where(ex => ex.GetType() == exceptionType);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToProvider()
    {
        // Arrange
        var query = new SearchStocksQuery("Apple");
        var cancellationToken = new CancellationToken();
        var expectedResults = new List<StockSearchResult>
        {
            new(
                new TickerSymbol("AAPL"),
                "Apple Inc.",
                new ExchangeCode("NSDA"),
                new CurrencyCode("USD")
            )
        }.AsReadOnly();

        _stockSearchProviderMock
            .Setup(x => x.SearchStocksAsync("Apple", cancellationToken))
            .ReturnsAsync(expectedResults);

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        _stockSearchProviderMock.Verify(
            x => x.SearchStocksAsync("Apple", cancellationToken),
            Times.Once
        );
    }

    #endregion
}
