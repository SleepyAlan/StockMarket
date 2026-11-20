using FluentAssertions;
using Moq;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetCompanyProfile;
using StockMarket.Application.Features.Stocks.GetStockDetails;
using StockMarket.Application.Features.Stocks.GetStockQuote;
using StockMarket.Domain.ValueObjects;
using Xunit;

namespace StockMarket.Application.Tests.StocksTests;

public class GetStockDetailsHandlerTests
{
    private readonly Mock<ICompanyProfileProvider> _profileProviderMock;
    private readonly Mock<IQuoteProvider> _quoteProviderMock;
    private readonly GetStockDetailsHandler _handler;

    public GetStockDetailsHandlerTests()
    {
        _profileProviderMock = new Mock<ICompanyProfileProvider>();
        _quoteProviderMock = new Mock<IQuoteProvider>();
        _handler = new GetStockDetailsHandler(_profileProviderMock.Object, _quoteProviderMock.Object);
    }

    #region Validation Tests

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
        var query = new GetStockDetailsQuery(Stock: null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
        exception.ParamName.Should().Be("Stock");
    }

    #endregion

    #region Null Combination Tests

    [Fact]
    public async Task HandleAsync_WhenBothProvidersReturnNull_ReturnsNull()
    {
        // Arrange
        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyProfileResult?)null);

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockQuote?)null);

        var stock = new StockReference(
            Symbol: new TickerSymbol("AAPL"),
            Exchange: new ExchangeCode("XNSD"));
        var query = new GetStockDetailsQuery(Stock: stock);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenProfileIsNullAndQuoteIsNotNull_ReturnsPartialResult()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("AAPL"),
            Exchange: new ExchangeCode("XNSD"));

        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyProfileResult?)null);

        var quote = new StockQuote(
            Stock: stock,
            Price: new Money(150.50m, new CurrencyCode("USD")),
            PreviousClose: new Money(149.75m, new CurrencyCode("USD")),
            AtThisTimeUtc: DateTimeOffset.UtcNow,
            Provider: "TestProvider",
            Delay: MarketDataDelay.RealTime);

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        var query = new GetStockDetailsQuery(Stock: stock);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Profile.Should().BeNull();
        result.Quote.Should().NotBeNull();
        result.Quote.Should().Be(quote);
        result.Stock.Should().Be(stock);
    }

    [Fact]
    public async Task HandleAsync_WhenProfileIsNotNullAndQuoteIsNull_ReturnsPartialResult()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("MSFT"),
            Exchange: new ExchangeCode("XNSD"));

        var profile = new CompanyProfileResult(
            Stock: stock,
            Name: "Microsoft Corporation",
            CountryCode: "US",
            Industry: "Software",
            Sector: "Technology",
            Description: "Technology company",
            WebsiteUrl: "https://microsoft.com");

        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockQuote?)null);

        var query = new GetStockDetailsQuery(Stock: stock);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Profile.Should().NotBeNull();
        result.Profile.Should().Be(profile);
        result.Quote.Should().BeNull();
        result.Stock.Should().Be(stock);
    }

    [Fact]
    public async Task HandleAsync_WhenBothProvidersReturnData_ReturnsCompleteResult()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("GOOGL"),
            Exchange: new ExchangeCode("XNSD"));

        var profile = new CompanyProfileResult(
            Stock: stock,
            Name: "Alphabet Inc.",
            CountryCode: "US",
            Industry: "Internet Services",
            Sector: "Technology",
            Description: "Technology company",
            WebsiteUrl: "https://google.com");

        var quote = new StockQuote(
            Stock: stock,
            Price: new Money(140.25m, new CurrencyCode("USD")),
            PreviousClose: new Money(139.50m, new CurrencyCode("USD")),
            AtThisTimeUtc: DateTimeOffset.UtcNow,
            Provider: "TestProvider",
            Delay: MarketDataDelay.RealTime);

        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        var query = new GetStockDetailsQuery(Stock: stock);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Stock.Should().Be(stock);
        result.Profile.Should().Be(profile);
        result.Quote.Should().Be(quote);
    }

    #endregion

    #region Concurrent Execution Tests

    [Fact]
    public async Task HandleAsync_CallsBothProvidersExactlyOnce()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("TSLA"),
            Exchange: new ExchangeCode("XNSD"));

        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyProfileResult?)null);

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockQuote?)null);

        var query = new GetStockDetailsQuery(Stock: stock);

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _profileProviderMock.Verify(
            x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _quoteProviderMock.Verify(
            x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PassesCorrectStockReferenceToProviders()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("META"),
            Exchange: new ExchangeCode("XNSD"));

        StockReference? capturedProfileStock = null;
        StockReference? capturedQuoteStock = null;

        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .Callback((StockReference s, CancellationToken _) => capturedProfileStock = s)
            .ReturnsAsync((CompanyProfileResult?)null);

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .Callback((StockReference s, CancellationToken _) => capturedQuoteStock = s)
            .ReturnsAsync((StockQuote?)null);

        var query = new GetStockDetailsQuery(Stock: stock);

        // Act
        await _handler.HandleAsync(query);

        // Assert
        capturedProfileStock.Should().Be(stock);
        capturedQuoteStock.Should().Be(stock);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToProviders()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("NFLX"),
            Exchange: new ExchangeCode("XNSD"));

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), cancellationToken))
            .ReturnsAsync((CompanyProfileResult?)null);

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), cancellationToken))
            .ReturnsAsync((StockQuote?)null);

        var query = new GetStockDetailsQuery(Stock: stock);

        // Act
        await _handler.HandleAsync(query, cancellationToken);

        // Assert
        _profileProviderMock.Verify(
            x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), cancellationToken),
            Times.Once);
        _quoteProviderMock.Verify(
            x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), cancellationToken),
            Times.Once);
    }

    #endregion

    #region Exception Propagation Tests

    [Fact]
    public async Task HandleAsync_WhenProfileProviderThrowsException_PropagatesException()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("JPM"),
            Exchange: new ExchangeCode("XNYM"));

        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Profile provider unavailable"));

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockQuote?)null);

        var query = new GetStockDetailsQuery(Stock: stock);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
        exception.Message.Should().Be("Profile provider unavailable");
    }

    [Fact]
    public async Task HandleAsync_WhenQuoteProviderThrowsException_PropagatesException()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("BA"),
            Exchange: new ExchangeCode("XNYM"));

        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyProfileResult?)null);

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Quote provider unavailable"));

        var query = new GetStockDetailsQuery(Stock: stock);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
        exception.Message.Should().Be("Quote provider unavailable");
    }

    [Fact]
    public async Task HandleAsync_WhenBothProvidersThrowExceptions_PropagatesFirstException()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("IBM"),
            Exchange: new ExchangeCode("XNYM"));

        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Profile provider error"));

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Quote provider error"));

        var query = new GetStockDetailsQuery(Stock: stock);

        // Act & Assert
        // Task.WhenAll will throw the first exception it encounters
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
        // The exception message should be one of the provider errors
        exception.Message.Should().BeOneOf("Profile provider error", "Quote provider error");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task HandleAsync_WhenCancellationTokenIsCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("AMZN"),
            Exchange: new ExchangeCode("XNSD"));

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), cancellationToken))
            .Returns(async () => 
            {
                cts.Cancel();
                await Task.Delay(100);
                throw new OperationCanceledException(cancellationToken);
            });

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        var query = new GetStockDetailsQuery(Stock: stock);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.HandleAsync(query, cancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WithDefaultCancellationToken_ExecutesSuccessfully()
    {
        // Arrange
        var stock = new StockReference(
            Symbol: new TickerSymbol("DIS"),
            Exchange: new ExchangeCode("XNYM"));

        var profile = new CompanyProfileResult(
            Stock: stock,
            Name: "The Walt Disney Company",
            CountryCode: "US",
            Industry: "Entertainment",
            Sector: "Media & Entertainment",
            Description: "Entertainment company",
            WebsiteUrl: "https://disney.com");

        var quote = new StockQuote(
            Stock: stock,
            Price: new Money(92.35m, new CurrencyCode("USD")),
            PreviousClose: new Money(91.80m, new CurrencyCode("USD")),
            AtThisTimeUtc: DateTimeOffset.UtcNow,
            Provider: "TestProvider",
            Delay: MarketDataDelay.RealTime);

        _profileProviderMock
            .Setup(x => x.GetCompanyProfileAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _quoteProviderMock
            .Setup(x => x.GetStockQuoteAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        var query = new GetStockDetailsQuery(Stock: stock);

        // Act - uses default CancellationToken parameter
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Profile.Should().Be(profile);
        result.Quote.Should().Be(quote);
    }

    #endregion
}
