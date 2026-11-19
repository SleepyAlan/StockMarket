using FluentAssertions;
using Moq;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetStockQuote;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Application.Tests.StocksTests;

public class GetStockQuoteHandlerTests
{
    private readonly Mock<IQuoteProvider> _quoteProviderMock;
    private readonly GetStockQuoteHandler _sut;
    public GetStockQuoteHandlerTests()
    {
        _quoteProviderMock = new Mock<IQuoteProvider>();
        _sut = new GetStockQuoteHandler(_quoteProviderMock.Object);
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

    [Fact]
    public async Task HandleAsync_WhenStockIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var query = new GetStockQuoteQuery(null!);

        // Act
        var action = () => _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("Stock");
    }

    [Fact]
    public async Task HandleAsync_WhenProviderReturnsValidResult_ReturnsResult()
    {
        // Arrange
        var stock = new StockReference(new TickerSymbol("AAPL"), new ExchangeCode("NSDA"));
        var query = new GetStockQuoteQuery(stock);
        var price = new Money(150.00m, new CurrencyCode("USD"));
        var previousClose = new Money(149.50m, new CurrencyCode("USD"));
        var expectedResult = new StockQuote(
            stock,
            price,
            previousClose,
            DateTimeOffset.UtcNow,
            "FinnHub",
            MarketDataDelay.RealTime
        );

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull().And.Be(expectedResult);
        _quoteProviderMock.Verify(
            x => x.GetStockQuoteAsync(stock, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WhenProviderReturnsNull_ReturnsNull()
    {
        // Arrange
        var stock = new StockReference(new TickerSymbol("AAPL"), new ExchangeCode("NSDA"));
        var query = new GetStockQuoteQuery(stock);

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockQuote?)null);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenProviderReturnsPreviousCloseAsNull_ReturnsQuoteWithNullPreviousClose()
    {
        // Arrange
        var stock = new StockReference(new TickerSymbol("AAPL"), new ExchangeCode("NSDA"));
        var query = new GetStockQuoteQuery(stock);
        var price = new Money(150.00m, new CurrencyCode("USD"));
        var expectedResult = new StockQuote(
            stock,
            price,
            null,
            DateTimeOffset.UtcNow,
            "FinnHub",
            MarketDataDelay.Delayed
        );

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PreviousClose.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationTokenIsCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var stock = new StockReference(new TickerSymbol("AAPL"), new ExchangeCode("NSDA"));
        var query = new GetStockQuoteQuery(stock);
        var cancellationToken = new CancellationToken(canceled: true);

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(stock, cancellationToken))
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
        var stock = new StockReference(new TickerSymbol("AAPL"), new ExchangeCode("NSDA"));
        var query = new GetStockQuoteQuery(stock);
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(stock, It.IsAny<CancellationToken>()))
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
        var stock = new StockReference(new TickerSymbol("AAPL"), new ExchangeCode("NSDA"));
        var query = new GetStockQuoteQuery(stock);
        var cancellationToken = new CancellationToken();
        var price = new Money(150.00m, new CurrencyCode("USD"));
        var expectedResult = new StockQuote(
            stock,
            price,
            null,
            DateTimeOffset.UtcNow,
            "FinnHub",
            MarketDataDelay.EndOfDay
        );

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(stock, cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        _quoteProviderMock.Verify(
            x => x.GetStockQuoteAsync(stock, cancellationToken),
            Times.Once
        );
    }

    #endregion
}
