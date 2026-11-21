using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarket.Domain.Entities;
using StockMarket.Domain.ValueObjects;

namespace StockMarket.Infrastructure.Persistence.Configurations;

public sealed class StockListingConfiguration : IEntityTypeConfiguration<StockListing>
{
    public void Configure(EntityTypeBuilder<StockListing> builder)
    {
        builder.ToTable("StockListings");

        builder.HasKey(x => x.StockListingId);
        builder.Property(x => x.StockListingId)
            .ValueGeneratedNever(); // Generating id in the domain layer, so EF should not generate it

        builder.Property(x => x.Symbol)
            .HasConversion(
                value => value.Value,
                value => new TickerSymbol(value))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Exchange)
            .HasConversion(
                value => value.Value,
                value => new ExchangeCode(value))
            .HasMaxLength(4)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasConversion(
                value => value.Value,
                value => new CurrencyCode(value))
            .HasMaxLength(3)
            .IsRequired();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.Exchange,
            x.Symbol
        })
        .IsUnique();
    }
}
