using FluentAssertions;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Domain.Tests.ValueObjectTests;

public class MoneyTests
{
    #region Constructor Tests

    [Theory]
    [InlineData(100.50, "USD")]
    [InlineData(0, "EUR")]
    [InlineData(-50.75, "JPY")]
    public void Constructor_ValidMoney_ShouldCreateInstance(decimal amount, string currencyCode)
    {
        // Arrange
        var currency = new CurrencyCode(currencyCode);
        // Act
        var money = new Money(amount, currency);
        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Fact]
    public void Constructor_NullCurrency_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new Money(100, null!);
        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void Addition_SameCurrency_ShouldReturnCorrectResult()
    {
        // Arrange
        var currency = new CurrencyCode("USD");
        var money1 = new Money(100, currency);
        var money2 = new Money(50, currency);
        // Act
        var result = money1 + money2;
        // Assert
        result.Amount.Should().Be(150);
        result.Currency.Should().Be(currency);
    }

    [Fact]
    public void Addition_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100, new CurrencyCode("USD"));
        var money2 = new Money(50, new CurrencyCode("EUR"));
        // Act
        Action act = () => { var result = money1 + money2; };
        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtraction_SameCurrency_ShouldReturnCorrectResult()
    {
        // Arrange
        var currency = new CurrencyCode("USD");
        var money1 = new Money(100, currency);
        var money2 = new Money(50, currency);
        // Act
        var result = money1 - money2;
        // Assert
        result.Amount.Should().Be(50);
        result.Currency.Should().Be(currency);
    }

    [Fact]
    public void Subtraction_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100, new CurrencyCode("USD"));
        var money2 = new Money(50, new CurrencyCode("EUR"));
        // Act
        Action act = () => { var result = money1 - money2; };
        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Division_SameCurrency_ShouldReturnCorrectResult()
    {
        // Arrange
        var currency = new CurrencyCode("USD");
        var money1 = new Money(100, currency);
        var money2 = new Money(50, currency);
        // Act
        var result = money1 / money2;
        // Assert
        result.Amount.Should().Be(2);
        result.Currency.Should().Be(currency);
    }

    [Fact]
    public void Division_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100, new CurrencyCode("USD"));
        var money2 = new Money(50, new CurrencyCode("EUR"));
        // Act
        Action act = () => { var result = money1 / money2; };
        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiplication_SameCurrency_ShouldReturnCorrectResult()
    {
        // Arrange
        var currency = new CurrencyCode("USD");
        var money1 = new Money(100, currency);
        var money2 = new Money(50, currency);
        // Act
        var result = money1 * money2;
        // Assert
        result.Amount.Should().Be(5000);
        result.Currency.Should().Be(currency);
    }

    [Fact]
    public void Multiplication_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100, new CurrencyCode("USD"));
        var money2 = new Money(50, new CurrencyCode("EUR"));
        // Act
        Action act = () => { var result = money1 * money2; };
        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion
}
