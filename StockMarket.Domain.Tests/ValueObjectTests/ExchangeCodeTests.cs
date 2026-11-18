using FluentAssertions;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Domain.Tests.ValueObjectTests;

public class ExchangeCodeTests
{
    #region Constructor Tests

    [Theory]
    [InlineData("XNAS")]
    [InlineData("xjse")]
    [InlineData(" lsee ")]
    [InlineData("HKE1")]
    [InlineData("1234")]
    public void Constructor_ValidExchangeCode_ShouldCreateInstance(string input)
    {
        // Arrange
        var expectedValue = input.Trim().ToUpperInvariant();
        // Act
        var exchangeCode = new ExchangeCode(input);
        // Assert
        exchangeCode.Value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("XNA")]
    [InlineData("XNAS1")]
    [InlineData("XNA$")]
    [InlineData("XNA ")]
    public void Constructor_InvalidExchangeCode_ShouldThrowArgumentException(string? input)
    {
        // Act
        Action act = () => new ExchangeCode(input!);
        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion
}
