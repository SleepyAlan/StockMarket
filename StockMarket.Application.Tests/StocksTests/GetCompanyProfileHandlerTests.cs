using FluentAssertions;
using Moq;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetCompanyProfile;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Application.Tests.StocksTests;

public class GetCompanyProfileHandlerTests
{
    private readonly Mock<ICompanyProfilerProvider> _companyProfilerProviderMock;
    private readonly GetCompanyProfileHandler _sut;

    public GetCompanyProfileHandlerTests()
    {
        _companyProfilerProviderMock = new Mock<ICompanyProfilerProvider>();
        _sut = new GetCompanyProfileHandler(_companyProfilerProviderMock.Object);
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
        var query = new GetCompanyProfileQuery(null!);

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
        var query = new GetCompanyProfileQuery(stock);
        var expectedResult = new CompanyProfileResult(
            stock,
            "Apple Inc.",
            "US",
            "Technology",
            "Information Technology",
            "Designs, manufactures, and markets smartphones, computers, and wearables.",
            "https://www.apple.com"
        );

        _companyProfilerProviderMock
            .Setup(x => x.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull().And.Be(expectedResult);
        _companyProfilerProviderMock.Verify(
            x => x.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WhenProviderReturnsNull_ReturnsNull()
    {
        // Arrange
        var stock = new StockReference(new TickerSymbol("AAPL"), new ExchangeCode("NSDA"));
        var query = new GetCompanyProfileQuery(stock);

        _companyProfilerProviderMock
            .Setup(x => x.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyProfileResult?)null);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationTokenIsCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var stock = new StockReference(new TickerSymbol("AAPL"), new ExchangeCode("NSDA"));
        var query = new GetCompanyProfileQuery(stock);
        var cancellationToken = new CancellationToken(canceled: true);

        _companyProfilerProviderMock
            .Setup(x => x.GetCompanyProfileAsync(stock, cancellationToken))
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
        var query = new GetCompanyProfileQuery(stock);
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        _companyProfilerProviderMock
            .Setup(x => x.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
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
        var query = new GetCompanyProfileQuery(stock);
        var cancellationToken = new CancellationToken();
        var expectedResult = new CompanyProfileResult(
            stock,
            "Apple Inc.",
            "US",
            "Technology",
            "Information Technology",
            "Designs, manufactures, and markets smartphones, computers, and wearables.",
            "https://www.apple.com"
        );

        _companyProfilerProviderMock
            .Setup(x => x.GetCompanyProfileAsync(stock, cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        _companyProfilerProviderMock.Verify(
            x => x.GetCompanyProfileAsync(stock, cancellationToken),
            Times.Once
        );
    }

    #endregion
}
