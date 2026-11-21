using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarket.Domain.Entities;

namespace StockMarket.Infrastructure.Persistence.Configurations;

public sealed class WatchlistItemConfiguration : IEntityTypeConfiguration<WatchlistItem>
{
    public void Configure(EntityTypeBuilder<WatchlistItem> builder)
    {
        builder.ToTable("WatchlistItems");

        builder.HasKey(x => x.WatchlistItemId);
        builder.Property(x => x.WatchlistItemId)
            .ValueGeneratedNever(); // Generating id in the domain layer, so EF should not generate it

        builder.HasOne<StockListing>()
            .WithMany()
            .HasForeignKey(x => x.StockListingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.WatchlistId,
            x.StockListingId
        })
        .IsUnique();
    }
}
