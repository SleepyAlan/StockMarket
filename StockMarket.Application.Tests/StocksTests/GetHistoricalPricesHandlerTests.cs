using FluentAssertions;
using Moq;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetHistoricalPrices;
using StockMarket.Domain.ValueObjects;
using Xunit;

namespace StockMarket.Application.Tests.StocksTests;

public class GetHistoricalPricesHandlerTests
{
    private readonly Mock<IHistoricalPriceProvider> _providerMock;
    private readonly GetHistoricalPricesHandler _handler;

    public GetHistoricalPricesHandlerTests()
    {
        _providerMock = new Mock<IHistoricalPriceProvider>();
        _handler = new GetHistoricalPricesHandler(_providerMock.Object);
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
    public async Task HandleAsync_WithNullStock_ThrowsArgumentNullException()
    {
        // Arrange
        var query = new GetHistoricalPricesQuery(
            Stock: null!,
            FromUtc: DateTimeOffset.UtcNow.AddDays(-30),
            ToUtc: DateTimeOffset.UtcNow,
            Interval: HistoricalPriceInterval.OneDay);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
        exception.ParamName.Should().Be("Stock");
    }

    [Theory]
    [InlineData((HistoricalPriceInterval)999)]
    [InlineData((HistoricalPriceInterval)(-1))]
    public async Task HandleAsync_WithUndefinedInterval_ThrowsArgumentOutOfRangeException(HistoricalPriceInterval invalidInterval)
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("AAPL"),
            Exchange: new ExchangeCode("XNSD"));

        var query = new GetHistoricalPricesQuery(
            Stock: stock,
            FromUtc: DateTimeOffset.UtcNow.AddDays(-30),
            ToUtc: DateTimeOffset.UtcNow,
            Interval: invalidInterval);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
        exception.ParamName.Should().Be("Interval");
    }

    [Theory]
    [InlineData((PriceAdjustmentMode)999)]
    [InlineData((PriceAdjustmentMode)(-1))]
    public async Task HandleAsync_WithUndefinedAdjustment_ThrowsArgumentOutOfRangeException(PriceAdjustmentMode invalidAdjustment)
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("AAPL"),
            Exchange: new ExchangeCode("XNSD"));

        var query = new GetHistoricalPricesQuery(
            Stock: stock,
            FromUtc: DateTimeOffset.UtcNow.AddDays(-30),
            ToUtc: DateTimeOffset.UtcNow,
            Interval: HistoricalPriceInterval.OneDay,
            Adjustment: invalidAdjustment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
        exception.ParamName.Should().Be("Adjustment");
    }

    [Fact]
    public async Task HandleAsync_WithFromUtcEqualToToUtc_ThrowsArgumentException()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("AAPL"),
            Exchange: new ExchangeCode("XNSD"));

        var sameTime = DateTimeOffset.UtcNow;
        var query = new GetHistoricalPricesQuery(
            Stock: stock,
            FromUtc: sameTime,
            ToUtc: sameTime,
            Interval: HistoricalPriceInterval.OneDay);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
        exception.Message.Should().Contain("date");
    }

    [Fact]
    public async Task HandleAsync_WithFromUtcGreaterThanToUtc_ThrowsArgumentException()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("AAPL"),
            Exchange: new ExchangeCode("XNSD"));

        var fromTime = DateTimeOffset.UtcNow;
        var toTime = fromTime.AddDays(-10);

        var query = new GetHistoricalPricesQuery(
            Stock: stock,
            FromUtc: fromTime,
            ToUtc: toTime,
            Interval: HistoricalPriceInterval.OneDay);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
        exception.Message.Should().Contain("date");
    }

    [Fact]
    public async Task HandleAsync_WithProviderReturningNull_ReturnsNull()
    {
        // Arrange
        _providerMock
            .Setup(x => x.GetHistoricalPricesAsync(It.IsAny<GetHistoricalPricesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HistoricalPriceSeries?)null);

        var stock = new StockReference(
            Symbol: new TickerSymbol("AAPL"),
            Exchange: new ExchangeCode("XNSD"));

        var query = new GetHistoricalPricesQuery(
            Stock: stock,
            FromUtc: DateTimeOffset.UtcNow.AddDays(-30),
            ToUtc: DateTimeOffset.UtcNow,
            Interval: HistoricalPriceInterval.OneDay);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithUnorderedBars_SortsByTimestampUtcAscending()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("AAPL"),
            Exchange: new ExchangeCode("XNSD"));

        var now = DateTimeOffset.UtcNow;
        var unorderedBars = new[]
        {
            new HistoricalPriceBar(now.AddDays(2), 150m, 155m, 148m, 154m, 1000000),
            new HistoricalPriceBar(now.AddDays(-1), 140m, 145m, 138m, 144m, 900000),
            new HistoricalPriceBar(now, 145m, 150m, 143m, 149m, 1100000),
            new HistoricalPriceBar(now.AddDays(1), 149m, 153m, 147m, 152m, 950000)
        };

        var series = new HistoricalPriceSeries(
            Stock: stock,
            Currency: new CurrencyCode("USD"),
            Interval: HistoricalPriceInterval.OneDay,
            Adjustment: PriceAdjustmentMode.Raw,
            Provider: "TestProvider",
            Bars: unorderedBars);

        _providerMock
            .Setup(x => x.GetHistoricalPricesAsync(It.IsAny<GetHistoricalPricesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(series);

        var query = new GetHistoricalPricesQuery(
            Stock: stock,
            FromUtc: now.AddDays(-30),
            ToUtc: now.AddDays(3),
            Interval: HistoricalPriceInterval.OneDay);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Bars.Should().BeInAscendingOrder(x => x.TimestampUtc);
        result.Bars.Select(x => x.TimestampUtc).Should()
            .ContainInOrder(now.AddDays(-1), now, now.AddDays(1), now.AddDays(2));
    }

    [Fact]
    public async Task HandleAsync_WithAlreadySortedBars_PreservesOrder()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("MSFT"),
            Exchange: new ExchangeCode("XNSD"));

        var now = DateTimeOffset.UtcNow;
        var sortedBars = new[]
        {
            new HistoricalPriceBar(now.AddDays(-2), 300m, 305m, 298m, 304m, 800000),
            new HistoricalPriceBar(now.AddDays(-1), 304m, 310m, 302m, 309m, 850000),
            new HistoricalPriceBar(now, 309m, 315m, 307m, 314m, 900000),
            new HistoricalPriceBar(now.AddDays(1), 314m, 318m, 312m, 317m, 750000)
        };

        var series = new HistoricalPriceSeries(
            Stock: stock,
            Currency: new CurrencyCode("USD"),
            Interval: HistoricalPriceInterval.OneDay,
            Adjustment: PriceAdjustmentMode.Raw,
            Provider: "TestProvider",
            Bars: sortedBars);

        _providerMock
            .Setup(x => x.GetHistoricalPricesAsync(It.IsAny<GetHistoricalPricesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(series);

        var stock2 = new StockReference(
            Symbol: new TickerSymbol("MSFT"),
            Exchange: new ExchangeCode("XNSD"));

        var query = new GetHistoricalPricesQuery(
            Stock: stock2,
            FromUtc: now.AddDays(-30),
            ToUtc: now.AddDays(3),
            Interval: HistoricalPriceInterval.OneDay);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Bars.Should().BeInAscendingOrder(x => x.TimestampUtc);
        result.Bars.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(HistoricalPriceInterval.OneMinute)]
    [InlineData(HistoricalPriceInterval.OneMonth)]
    public async Task HandleAsync_WithRepresentativeIntervals_SuccessfullyRetrievesData(HistoricalPriceInterval interval)
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("GOOGL"),
            Exchange: new ExchangeCode("XNSD"));

        var now = DateTimeOffset.UtcNow;
        var bars = new[]
        {
            new HistoricalPriceBar(now, 2800m, 2850m, 2790m, 2840m, 5000000)
        };

        var series = new HistoricalPriceSeries(
            Stock: stock,
            Currency: new CurrencyCode("USD"),
            Interval: interval,
            Adjustment: PriceAdjustmentMode.Raw,
            Provider: "TestProvider",
            Bars: bars);

        _providerMock
            .Setup(x => x.GetHistoricalPricesAsync(It.IsAny<GetHistoricalPricesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(series);

        var query = new GetHistoricalPricesQuery(
            Stock: stock,
            FromUtc: now.AddDays(-30),
            ToUtc: now.AddDays(1),
            Interval: interval);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Interval.Should().Be(interval);
        result.Bars.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(PriceAdjustmentMode.Raw)]
    [InlineData(PriceAdjustmentMode.SplitAndDividendAdjusted)]
    public async Task HandleAsync_WithRepresentativeAdjustments_SuccessfullyRetrievesData(PriceAdjustmentMode adjustment)
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("JPM"),
            Exchange: new ExchangeCode("XNYM"));

        var now = DateTimeOffset.UtcNow;
        var bars = new[]
        {
            new HistoricalPriceBar(now, 150m, 155m, 148m, 154m, 2000000)
        };

        var series = new HistoricalPriceSeries(
            Stock: stock,
            Currency: new CurrencyCode("USD"),
            Interval: HistoricalPriceInterval.OneDay,
            Adjustment: adjustment,
            Provider: "TestProvider",
            Bars: bars);

        _providerMock
            .Setup(x => x.GetHistoricalPricesAsync(It.IsAny<GetHistoricalPricesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(series);

        var query = new GetHistoricalPricesQuery(
            Stock: stock,
            FromUtc: now.AddDays(-30),
            ToUtc: now.AddDays(1),
            Interval: HistoricalPriceInterval.OneDay,
            Adjustment: adjustment);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Adjustment.Should().Be(adjustment);
    }

    [Fact]
    public async Task HandleAsync_NormalizesTimestampsToUtc()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("TSLA"),
            Exchange: new ExchangeCode("XNSD"));

        var utcNow = DateTimeOffset.UtcNow;
        var localFromTime = new DateTimeOffset(utcNow.AddDays(-30).DateTime, TimeSpan.FromHours(5));
        var localToTime = new DateTimeOffset(utcNow.DateTime, TimeSpan.FromHours(5));

        GetHistoricalPricesQuery? capturedQuery = null;
        _providerMock
            .Setup(x => x.GetHistoricalPricesAsync(It.IsAny<GetHistoricalPricesQuery>(), It.IsAny<CancellationToken>()))
            .Callback((GetHistoricalPricesQuery q, CancellationToken _) => capturedQuery = q)
            .ReturnsAsync(new HistoricalPriceSeries(
                Stock: stock,
                Currency: new CurrencyCode("USD"),
                Interval: HistoricalPriceInterval.OneDay,
                Adjustment: PriceAdjustmentMode.Raw,
                Provider: "TestProvider",
                Bars: []));

        var query = new GetHistoricalPricesQuery(
            Stock: stock,
            FromUtc: localFromTime,
            ToUtc: localToTime,
            Interval: HistoricalPriceInterval.OneDay);

        // Act
        await _handler.HandleAsync(query);

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.FromUtc.Offset.Should().Be(TimeSpan.Zero);
        capturedQuery.ToUtc.Offset.Should().Be(TimeSpan.Zero);
        capturedQuery.FromUtc.Should().Be(localFromTime.ToUniversalTime());
        capturedQuery.ToUtc.Should().Be(localToTime.ToUniversalTime());
    }

    [Fact]
    public async Task HandleAsync_WhenProviderThrowsException_PropagatesException()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("META"),
            Exchange: new ExchangeCode("XNSD"));

        _providerMock
            .Setup(x => x.GetHistoricalPricesAsync(It.IsAny<GetHistoricalPricesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Provider service unavailable"));

        var query = new GetHistoricalPricesQuery(
            Stock: stock,
            FromUtc: DateTimeOffset.UtcNow.AddDays(-30),
            ToUtc: DateTimeOffset.UtcNow,
            Interval: HistoricalPriceInterval.OneDay);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
        exception.Message.Should().Be("Provider service unavailable");
    }

    [Fact]
    public async Task HandleAsync_WithValidQueryAndData_CancellationTokenPropagates()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("NFLX"),
            Exchange: new ExchangeCode("XNSD"));

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var now = DateTimeOffset.UtcNow;
        var series = new HistoricalPriceSeries(
            Stock: stock,
            Currency: new CurrencyCode("USD"),
            Interval: HistoricalPriceInterval.OneDay,
            Adjustment: PriceAdjustmentMode.Raw,
            Provider: "TestProvider",
            Bars: [new HistoricalPriceBar(now, 200m, 205m, 198m, 204m, 1500000)]);

        _providerMock
            .Setup(x => x.GetHistoricalPricesAsync(It.IsAny<GetHistoricalPricesQuery>(), cancellationToken))
            .ReturnsAsync(series);

        var query = new GetHistoricalPricesQuery(
            Stock: stock,
            FromUtc: now.AddDays(-30),
            ToUtc: now.AddDays(1),
            Interval: HistoricalPriceInterval.OneDay);

        // Act
        var result = await _handler.HandleAsync(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        _providerMock.Verify(
            x => x.GetHistoricalPricesAsync(It.IsAny<GetHistoricalPricesQuery>(), cancellationToken),
            Times.Once);
    }

    #endregion
}
