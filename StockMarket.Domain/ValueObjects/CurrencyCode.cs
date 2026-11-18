namespace StockMarket.Domain.ValueObjects;

public record CurrencyCode
{
    public string Value { get; } = string.Empty;

    public CurrencyCode(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));

        var normalized = value.Trim().ToUpperInvariant();
        
        if (normalized.Length != 3 || normalized.Any(c => !char.IsLetter(c)))
        {
            throw new ArgumentException("Invalid currency code: must be exactly 3 letters.", nameof(value));
        }
        Value = normalized;
    }

    public override string ToString() => Value;
}
