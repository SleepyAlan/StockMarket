using FluentAssertions;
using Moq;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Application.Abstractions.StockMarketData;
using StockMarket.Application.Common.Models;
using StockMarket.Application.Features.Stocks.GetCompanyProfile;
using StockMarket.Application.Features.Stocks.ResolveOrRegisterStockListing;
using StockMarket.Domain.Entities;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Application.Tests.StocksTests;

public class ResolveOrRegisterStockListingHandlerTests
{
    private readonly Mock<IStockCatalogRepository> _repositoryMock;
    private readonly Mock<IStockListingMetadataProvider> _stockListingProviderMock;
    private readonly Mock<ICompanyProfileProvider> _companyProfileProviderMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ResolveOrRegisterStockListingHandler _sut;

    public ResolveOrRegisterStockListingHandlerTests()
    {
        _repositoryMock = new Mock<IStockCatalogRepository>();
        _stockListingProviderMock = new Mock<IStockListingMetadataProvider>();
        _companyProfileProviderMock = new Mock<ICompanyProfileProvider>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new ResolveOrRegisterStockListingHandler(
            _repositoryMock.Object,
            _stockListingProviderMock.Object,
            _companyProfileProviderMock.Object,
            _unitOfWorkMock.Object);
    }

    #region Null Argument Validation Tests

    [Fact]
    public async Task HandleAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        ResolveOrRegisterStockListingCommand? command = null;
        // Act
        var act = () => _sut.HandleAsync(command!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task HandleAsync_WithNullStock_ThrowsArgumentNullException()
    {
        // Arrange
        var command = new ResolveOrRegisterStockListingCommand(null!);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task HandleAsync_WithNullSymbol_ThrowsArgumentNullException()
    {
        // Arrange
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(null!, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task HandleAsync_WithNullExchange_ThrowsArgumentNullException()
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var stock = new StockReference(symbol, null!);
        var command = new ResolveOrRegisterStockListingCommand(stock);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Existing Stock Lookup Tests

    [Fact]
    public async Task HandleAsync_WhenStockExists_ReturnsExistingId()
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);
        var existingId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingId);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().Be(existingId);
        _repositoryMock.Verify(
            r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()),
            Times.Once);
        _stockListingProviderMock.Verify(
            p => p.ResolveAsync(It.IsAny<StockReference>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Provider Resolution Tests

    [Fact]
    public async Task HandleAsync_WhenProviderReturnsNull_ThrowsKeyNotFoundException()
    {
        // Arrange
        var symbol = new TickerSymbol("INVALID");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VerifiedStockListing?)null);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenProviderReturnsDifferentStock_ThrowsInvalidOperationException()
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);

        var differentSymbol = new TickerSymbol("MSFT");
        var differentStock = new StockReference(differentSymbol, exchange);
        var verified = new VerifiedStockListing(differentStock, new CurrencyCode("USD"), "MSFT", "Provider");

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region Company Profile Validation Tests

    [Fact]
    public async Task HandleAsync_WhenProfileIsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);

        var verified = new VerifiedStockListing(
            stock, new CurrencyCode("USD"),
            "AAPL", "Provider");

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        _companyProfileProviderMock
            .Setup(p => p.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyProfileResult?)null);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenProfileStockMismatches_ThrowsInvalidOperationException()
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);

        var verified = new VerifiedStockListing(
            stock,
            new CurrencyCode("USD"),
            "AAPL",
            "Provider");

        var differentSymbol = new TickerSymbol("MSFT");
        var differentStock = new StockReference(differentSymbol, exchange);
        var profile = new CompanyProfileResult(
            differentStock,
            "Apple Inc.",
            "US",
            "Technology",
            "Consumer Electronics",
            "Apple designs and manufactures devices.",
            "https://www.apple.com");

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        _companyProfileProviderMock
            .Setup(p => p.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WhenProfileNameInvalid_ThrowsInvalidOperationException(string? name)
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);

        var verified = new VerifiedStockListing(
            stock,
            new CurrencyCode("USD"),
            "AAPL",
            "Provider");

        var profile = new CompanyProfileResult(
            stock,
            name!,
            "US",
            "Technology",
            "Consumer Electronics",
            "Apple designs and manufactures devices.",
            "https://www.apple.com");

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        _companyProfileProviderMock
            .Setup(p => p.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Verified company information is unavailable.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WhenProfileCountryCodeInvalid_ThrowsInvalidOperationException(
        string? countryCode)
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);

        var verified = new VerifiedStockListing(
            stock,
            new CurrencyCode("USD"),
            "AAPL",
            "Provider");

        var profile = new CompanyProfileResult(
            stock,
            "Apple Inc.",
            countryCode,
            "Technology",
            "Consumer Electronics",
            "Apple designs and manufactures devices.",
            "https://www.apple.com");

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        _companyProfileProviderMock
            .Setup(p => p.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenProfileHasAllValidFields_SucceedsAsync()
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);
        var currency = new CurrencyCode("USD");

        var verified = new VerifiedStockListing(
            stock,
            currency,
            "AAPL",
            "Provider");

        var profile = new CompanyProfileResult(
            stock,
            "Apple Inc.",
            "US",
            "Technology",
            "Consumer Electronics",
            "Apple designs and manufactures devices.",
            "https://www.apple.com");

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        _companyProfileProviderMock
            .Setup(p => p.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenProfileHasMinimalValidFields_SucceedsAsync()
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);
        var currency = new CurrencyCode("USD");

        var verified = new VerifiedStockListing(
            stock,
            currency,
            "AAPL",
            "Provider");

        var profile = new CompanyProfileResult(
            stock,
            "Apple Inc.",
            "US",
            null,
            null,
            null,
            null);

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        _companyProfileProviderMock
            .Setup(p => p.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion

    #region Race Condition Mitigation Tests

    [Fact]
    public async Task HandleAsync_WhenRaceConditionOccurs_ReturnsExistingId()
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);
        var currency = new CurrencyCode("USD");
        var racedId = Guid.NewGuid();

        var verified = new VerifiedStockListing(
            stock,
            currency,
            "AAPL",
            "Provider");

        var profile = new CompanyProfileResult(
            stock,
            "Apple Inc.",
            "US",
            "Technology",
            "Consumer Electronics",
            "Apple designs and manufactures devices.",
            "https://www.apple.com");

        // First call to GetListingIdAsync returns null, second call returns the raced ID
        _repositoryMock
            .SetupSequence(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null)
            .ReturnsAsync(racedId);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        _companyProfileProviderMock
            .Setup(p => p.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().Be(racedId);
        _repositoryMock.Verify(
            r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _repositoryMock.Verify(
            r => r.AddCompanyAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repositoryMock.Verify(
            r => r.AddStockListingAsync(It.IsAny<StockListing>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Successful return Tests

    [Fact]
    public async Task HandleAsync_WithValidNewStock_RegistersSuccessfully()
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);
        var currency = new CurrencyCode("USD");

        var verified = new VerifiedStockListing(
            stock,
            currency,
            "AAPL",
            "Provider");

        var profile = new CompanyProfileResult(
            stock,
            "Apple Inc.",
            "US",
            "Technology",
            "Consumer Electronics",
            "Apple innovates in consumer electronics.",
            "https://www.apple.com");

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        _companyProfileProviderMock
            .Setup(p => p.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _repositoryMock.Verify(
            r => r.AddCompanyAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _repositoryMock.Verify(
            r => r.AddStockListingAsync(It.IsAny<StockListing>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CreatesCompanyWithCorrectData()
    {
        // Arrange
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);
        var currency = new CurrencyCode("USD");

        var verified = new VerifiedStockListing(
            stock,
            currency,
            "AAPL",
            "Provider");

        var profile = new CompanyProfileResult(
            stock,
            "Apple Inc.",
            "US",
            "Technology",
            "Consumer Electronics",
            "Apple innovates in consumer electronics.",
            "https://www.apple.com");

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        _companyProfileProviderMock
            .Setup(p => p.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.AddCompanyAsync(
                It.Is<Company>(c =>
                    c.Name == "Apple Inc." &&
                    c.CountryCode == "US"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CreatesStockListingWithCorrectData()
    {
        // Arrange
        var symbol = new TickerSymbol("MSFT");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);
        var currency = new CurrencyCode("USD");

        var verified = new VerifiedStockListing(
            stock,
            currency,
            "MSFT",
            "Provider");

        var profile = new CompanyProfileResult(
            stock,
            "Microsoft Corporation",
            "US",
            "Technology",
            "Software",
            "Microsoft develops software and cloud services.",
            "https://www.microsoft.com");

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        _companyProfileProviderMock
            .Setup(p => p.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.AddStockListingAsync(
                It.Is<StockListing>(s =>
                    s.Symbol == symbol &&
                    s.Exchange == exchange &&
                    s.Currency == currency),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ReturnsCreatedStockListingId()
    {
        // Arrange
        var symbol = new TickerSymbol("GOOGL");
        var exchange = new ExchangeCode("XNAS");
        var stock = new StockReference(symbol, exchange);
        var command = new ResolveOrRegisterStockListingCommand(stock);
        var currency = new CurrencyCode("USD");

        var verified = new VerifiedStockListing(
            stock,
            currency,
            "GOOGL",
            "Provider");

        var profile = new CompanyProfileResult(
            stock,
            "Alphabet Inc.",
            "US",
            "Technology",
            "Internet Services",
            "Alphabet is a holding company for technology companies.",
            "https://www.google.com");

        _repositoryMock
            .Setup(r => r.GetListingIdAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _stockListingProviderMock
            .Setup(p => p.ResolveAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        _companyProfileProviderMock
            .Setup(p => p.GetCompanyProfileAsync(stock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    #endregion
}
