using FluentAssertions;
using StockMarket.Domain.Entities;

namespace StockMarket.Domain.Tests.EntitiesTests;

public class CompanyTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_GivenValidParameters_ShouldCreateCompany()
    {
        // Arrange
        var name = "Test Company";
        var countryCode = "US";
        // Act
        var company = new Company(name, countryCode);
        // Assert
        company.CompanyId.Should().NotBeEmpty();
        company.Name.Should().Be(name);
        company.CountryCode.Should().Be(countryCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_GivenInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var countryCode = "US";
        // Act
        Action act = () => new Company(invalidName!, countryCode);
        // Assert
        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Constructor_GivenNameLongerThan200Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var name = new string('A', 201);
        var countryCode = "US";
        // Act
        Action act = () => new Company(name, countryCode);
        // Assert
        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("U")] // Too short
    [InlineData("USA")] // Too long
    [InlineData("1A")] // Contains a digit
    [InlineData("A!")] // Contains a special character
    [InlineData("A ")] // Contains a space
    [InlineData(" A")]
    public void Constructor_GivenInvalidCountryCode_ShouldThrowArgumentException(string? invalidCountryCode)
    {
        // Arrange
        var name = "Test Company";
        // Act
        Action act = () => new Company(name, invalidCountryCode!);
        // Assert
        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("countryCode");
    }
    #endregion

    #region UpdateCompanyProfile Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateCompanyProfile_GivenInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var company = new Company("Valid Name", "US");
        // Act
        Action act = () => company.UpdateCompanyProfile(invalidName!, null, null, null, null);
        // Assert
        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
    }

    [Fact]
    public void UpdateCompanyProfile_GivenNameLongerThan200Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var company = new Company("Valid Name", "US");
        var longName = new string('A', 201);
        // Act
        Action act = () => company.UpdateCompanyProfile(longName, null, null, null, null);
        // Assert
        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
    }

    [Fact]
    public void UpdateCompanyProfile_GivenValidParameters_ShouldUpdateCompanyProfile()
    {
        // Arrange
        var company = new Company("Valid Name", "US");
        var newName = "Updated Name";
        var newSector = "Technology";
        var newIndustry = "Software";
        var newDescription = "A leading software company.";
        var newWebsiteUrl = "https://www.updatedcompany.com";
        // Act
        company.UpdateCompanyProfile(newName, newSector, newIndustry, newDescription, newWebsiteUrl);
        // Assert
        company.Name.Should().Be(newName);
        company.Sector.Should().Be(newSector);
        company.Industry.Should().Be(newIndustry);
        company.Description.Should().Be(newDescription);
        company.Website.Should().Be(newWebsiteUrl);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("  Technology  ", "Technology")]
    [InlineData("  Software  ", "Software")]
    public void UpdateCompanyProfile_ShouldNormalizeOptionalParameters(string? input, string? expected)
    {
        // Arrange
        var company = new Company("Valid Name", "US");
        // Act
        company.UpdateCompanyProfile("Valid Name", input, input, input, null);
        // Assert
        company.Sector.Should().Be(expected);
        company.Industry.Should().Be(expected);
        company.Description.Should().Be(expected);
    }

    [Fact]
    public void UpdateCompanyProfile_GivenValidWebsiteUrl_ShouldUpdateWebsite()
    {
        // Arrange
        var company = new Company("Valid Name", "US");
        var newWebsiteUrl = "https://www.updatedcompany.com";
        // Act
        company.UpdateCompanyProfile("Valid Name", null, null, null, newWebsiteUrl);
        // Assert
        company.Website.Should().Be(newWebsiteUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateCompanyProfile_GivenNullOrWhitespaceWebsiteUrl_ShouldSetWebsiteToNull(string? websiteUrl)
    {
        // Arrange
        var company = new Company("Valid Name", "US");
        // Act
        company.UpdateCompanyProfile("Valid Name", null, null, null, websiteUrl);
        // Assert
        company.Website.Should().BeNull();
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("http:/invalid-url.com")]
    [InlineData("https://")]
    [InlineData("ftp://invalid-url.com")]
    [InlineData("www.invalid-url.com")]
    public void UpdateCompanyProfile_GivenInvalidWebsiteUrl_ShouldThrowArgumentException(string? invalidWebsiteUrl)
    {
        // Arrange
        var company = new Company("Valid Name", "US");
        // Act
        Action act = () => company.UpdateCompanyProfile("Valid Name", null, null, null, invalidWebsiteUrl);
        // Assert
        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("websiteUrl");
    }

    #endregion
}
