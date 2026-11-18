using FluentAssertions;
using StockMarket.Domain.Entities;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Domain.Tests.EntitiesTests;

public class StockListingTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_GivenValidParameters_ShouldCreateStockListing()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var currencyCode = new CurrencyCode("USD");

        // Act
        var stockListing = new StockListing(companyId, symbol, exchange, currencyCode);

        // Assert
        stockListing.CompanyId.Should().Be(companyId);
        stockListing.Symbol.Should().Be(symbol);
        stockListing.Exchange.Should().Be(exchange);
        stockListing.CurrencyCode.Should().Be(currencyCode);
        stockListing.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_GivenEmptyCompanyId_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyCompanyId = Guid.Empty;
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var currencyCode = new CurrencyCode("USD");

        // Act
        Action act = () => new StockListing(emptyCompanyId, symbol, exchange, currencyCode);

        // Assert
        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("companyId");
    }

    [Fact]
    public void Constructor_GivenNullSymbol_ShouldThrowArgumentNullException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        TickerSymbol? nullSymbol = null;
        var exchange = new ExchangeCode("XNAS");
        var currencyCode = new CurrencyCode("USD");

        // Act
        Action act = () => new StockListing(companyId, nullSymbol!, exchange, currencyCode);

        // Assert
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("symbol");
    }

    [Fact]
    public void Constructor_GivenNullExchange_ShouldThrowArgumentNullException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var symbol = new TickerSymbol("AAPL");
        ExchangeCode? nullExchange = null;
        var currencyCode = new CurrencyCode("USD");

        // Act
        Action act = () => new StockListing(companyId, symbol, nullExchange!, currencyCode);

        // Assert
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exchange");
    }

    [Fact]
    public void Constructor_GivenNullCurrencyCode_ShouldThrowArgumentNullException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        CurrencyCode? nullCurrencyCode = null;
        var currencyCode = new CurrencyCode("USD");

        // Act
        Action act = () => new StockListing(companyId, symbol, exchange, nullCurrencyCode!);

        // Assert
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("currencyCode");
    }

    #endregion

    #region ChangeSymbol Tests

    [Fact]
    public void ChangeSymbol_GivenValidNewSymbol_ShouldUpdateSymbol()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var currencyCode = new CurrencyCode("USD");
        var stockListing = new StockListing(companyId, symbol, exchange, currencyCode);
        var newSymbol = new TickerSymbol("GOOGL");

        // Act
        stockListing.ChangeSymbol(newSymbol);

        // Assert
        stockListing.Symbol.Should().Be(newSymbol);
    }

    [Fact]
    public void ChangeSymbol_GivenNullNewSymbol_ShouldThrowArgumentNullException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var currencyCode = new CurrencyCode("USD");
        var stockListing = new StockListing(companyId, symbol, exchange, currencyCode);
        TickerSymbol? nullNewSymbol = null;

        // Act
        Action act = () => stockListing.ChangeSymbol(nullNewSymbol!);

        // Assert
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("newSymbol");
    }

    #endregion

    #region Reactivate/Deactivate Tests

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var currencyCode = new CurrencyCode("USD");
        var stockListing = new StockListing(companyId, symbol, exchange, currencyCode);

        // Act
        stockListing.Deactivate();

        // Assert
        stockListing.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Reactivate_GivenDeactivatedStockListing_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var symbol = new TickerSymbol("AAPL");
        var exchange = new ExchangeCode("XNAS");
        var currencyCode = new CurrencyCode("USD");
        var stockListing = new StockListing(companyId, symbol, exchange, currencyCode);
        stockListing.Deactivate();

        // Act
        stockListing.Reactivate();

        // Assert
        stockListing.IsActive.Should().BeTrue();
    }

    #endregion
}
