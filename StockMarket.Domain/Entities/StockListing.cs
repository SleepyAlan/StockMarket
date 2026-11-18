using StockMarket.Domain.ValueObjects;

namespace StockMarket.Domain.Entities;

public class StockListing
{
    public  Guid StockListingId { get; private set; }
    public Guid CompanyId { get; private set; }
    public TickerSymbol Symbol { get; private set; } = null!;
    public ExchangeCode Exchange { get; private set; } = null!;
    public CurrencyCode CurrencyCode { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private StockListing() { }

    public StockListing(
        Guid companyId, TickerSymbol symbol,
        ExchangeCode exchange, CurrencyCode currencyCode)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException("Invalid company ID: cannot be empty.", nameof(companyId));
        }
        ArgumentNullException.ThrowIfNull(symbol, nameof(symbol));
        ArgumentNullException.ThrowIfNull(exchange, nameof(exchange));
        ArgumentNullException.ThrowIfNull(currencyCode, nameof(currencyCode));

        StockListingId = Guid.CreateVersion7();
        CompanyId = companyId;

        Symbol = symbol;
        Exchange = exchange;
        CurrencyCode = currencyCode;

        IsActive = true;
    }

    public void ChangeSymbol(TickerSymbol newSymbol)
    {
        ArgumentNullException.ThrowIfNull(newSymbol);

        Symbol = newSymbol;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }
}
