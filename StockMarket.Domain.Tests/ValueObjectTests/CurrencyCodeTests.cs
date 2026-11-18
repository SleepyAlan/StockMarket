using FluentAssertions;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Domain.Tests.ValueObjectTests;

public class CurrencyCodeTests
{
    #region Constructor Tests

    [Theory]
    [InlineData("USD")]
    [InlineData("eur")]
    [InlineData(" jpy ")]
    [InlineData("Gbp")]
    public void Constructor_ValidCurrencyCode_ShouldCreateInstance(string input)
    {
        // Arrange
        var expectedValue = input.Trim().ToUpperInvariant();
        // Act
        var currencyCode = new CurrencyCode(input);
        // Assert
        currencyCode.Value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("USDA")]
    [InlineData("US1")]
    [InlineData("123")]
    [InlineData("US$")]
    public void Constructor_InvalidCurrencyCode_ShouldThrowArgumentException(string? input)
    {
        // Act
        Action act = () => new CurrencyCode(input!);
        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion
}
