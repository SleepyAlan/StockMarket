using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Domain.Entities;
using StockMarket.Infrastructure.Identity;

namespace StockMarket.Infrastructure.Persistence;

public sealed class ApplicationDbContext : 
    IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>,
    IUnitOfWork // Implemented through the IdentityDbContext base class
{
    public DbSet<Company> Companies => Set<Company>(); 
    public DbSet<StockListing> StockListings => Set<StockListing>(); 
    public DbSet<Watchlist> Watchlists => Set<Watchlist>(); 
    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    }

}
