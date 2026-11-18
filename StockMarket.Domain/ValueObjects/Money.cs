namespace StockMarket.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; private set; }
    public CurrencyCode Currency { get; private set; } = null!;

    public Money(decimal amount, CurrencyCode currency)
    {
        ArgumentNullException.ThrowIfNull(currency, nameof(currency));

        Currency = currency;
        Amount = amount;
    }

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator /(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount / right.Amount, left.Currency);
    }

    public static Money operator *(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount * right.Amount, left.Currency);
    }

    public override string ToString() => $"{Amount} {Currency}";

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException("Cannot operate on Money with different currency codes.");
        }
    }
}
