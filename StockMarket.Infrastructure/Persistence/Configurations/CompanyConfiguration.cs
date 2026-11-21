using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarket.Domain.Entities;

namespace StockMarket.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(x => x.CompanyId);
        builder.Property(x => x.CompanyId)
            .ValueGeneratedNever(); // Generating id in the domain layer, so EF should not generate it

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CountryCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(x => x.Sector)
            .HasMaxLength(150);

        builder.Property(x => x.Industry)
            .HasMaxLength(150);

        builder.Property(x => x.Website)
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .HasMaxLength(4000);
    }
}
