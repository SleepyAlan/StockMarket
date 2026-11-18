namespace StockMarket.Domain.Entities;

public class Company
{
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty; // ISO 3166-1 Alpha-2 Codes
    public string? Description { get; private set; }
    public string? Website { get; private set; }
    public string? Industry { get; private set; }
    public string? Sector { get; private set; }

    private Company() { }

    public Company(string name, string countryCode)
    {
        CompanyId = Guid.CreateVersion7();
        Name = ValidateName(name);
        CountryCode = ValidateCountryCode(countryCode);
    }

    public void UpdateCompanyProfile(
        string name, string? sector,
        string? industry, string? description,
        string? websiteUrl)
    {
        Name = ValidateName(name);
        Website = ValidateWebsite(websiteUrl);

        Sector = NormalizeOptional(sector);
        Industry = NormalizeOptional(industry);
        Description = NormalizeOptional(description);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    #region Validation Methods
    private static string ValidateCountryCode(string countryCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        string normalized = countryCode.Trim().ToUpperInvariant();

        if (normalized.Length != 2 || normalized.Any(c => !char.IsLetter(c)))
        {
            throw new ArgumentException("Invalid ISO country code.");
        }

        return normalized;
    }

    private static string ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        string normalized = name.Trim();

        if (normalized.Length > 200)
        {
            throw new ArgumentException("Company name cannot exceed 200 characters.");
        }

        return normalized;
    }

    private static string? ValidateWebsite(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        string url = value.Trim();

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Website must be a valid HTTP or HTTPS URL.");
        }

        return url;
    }
    #endregion
}
