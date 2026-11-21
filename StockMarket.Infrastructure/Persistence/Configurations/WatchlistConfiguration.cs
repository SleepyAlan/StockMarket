using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarket.Domain.Entities;
using StockMarket.Infrastructure.Identity;
namespace StockMarket.Infrastructure.Persistence.Configurations;

public sealed class WatchlistConfiguration : IEntityTypeConfiguration<Watchlist>
{
    public void Configure(EntityTypeBuilder<Watchlist> builder)
    {
        builder.ToTable("Watchlists");

        builder.HasKey(x => x.WatchlistId);
        builder.Property(x => x.WatchlistId)
            .ValueGeneratedNever(); // Generating id in the domain layer, so EF should not generate it

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.WatchlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.Name
        })
        .IsUnique();
    }
}
