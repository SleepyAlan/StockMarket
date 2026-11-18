namespace StockMarket.Domain.ValueObjects;

public record ExchangeCode
{
    // ISO 10383 Market Identifier Code (MIC) format.
    public string Value { get; } = string.Empty;

    public ExchangeCode(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length != 4 || normalized.Any(c => !char.IsLetterOrDigit(c)))
        {
            throw new ArgumentException("Invalid exchange code: must be exactly 4 characters and contain only letters or digits.", nameof(value));
        }
        Value = normalized;
    }

    public override string ToString() => Value;
}
