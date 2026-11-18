using System.Text.RegularExpressions;

namespace StockMarket.Domain.ValueObjects;

public partial record TickerSymbol
{
    public string Value { get; } = string.Empty;

    public TickerSymbol(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));

        var normalized = value.Trim().ToUpperInvariant();
        if (!ValidFormatRegex().IsMatch(normalized))
        {
            throw new ArgumentException("Invalid ticker symbol.", nameof(value));
        }
        Value = normalized;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[A-Z0-9][A-Z0-9./-]{0,19}$")]
    private static partial Regex ValidFormatRegex();
}
