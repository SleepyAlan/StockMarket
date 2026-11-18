using FluentAssertions;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Domain.Tests.ValueObjectTests;

public class TickerSymbolTests
{
    #region Constructor Tests

    [Theory]
    [InlineData("AAPL")]
    [InlineData("GOOGL")]
    [InlineData("FB")]
    [InlineData("BRK.A")]
    [InlineData("BRK-B")]
    [InlineData("TSLA/US")]
    public void Constructor_ValidTickerSymbol_ShouldCreateInstance(string value)
    {
        // Act
        var tickerSymbol = new TickerSymbol(value);
        // Assert
        tickerSymbol.Value.Should().Be(value.ToUpperInvariant());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("INVALID SYMBOL")]
    [InlineData("123456789012345678901")]
    [InlineData("!@#$%^&*()")]
    [InlineData("TOO-LONG-TICKER-SYMBOL-EXCEEDING-LIMIT")]
    public void Constructor_InvalidTickerSymbol_ShouldThrowArgumentException(string? value)
    {
        // Act
        Action act = () => new TickerSymbol(value!);
        // Assert
        act.Should().Throw<ArgumentException>();
    }
    #endregion
}
