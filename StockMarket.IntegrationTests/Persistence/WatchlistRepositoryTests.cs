using Microsoft.EntityFrameworkCore;
using StockMarket.Application.Abstractions.Persistence;
using StockMarket.Domain.Entities;
using StockMarket.Domain.ValueObjects;
using StockMarket.Infrastructure.Identity;
using StockMarket.Infrastructure.Persistence.Repositories;
using StockMarket.IntegrationTests.Fixtures;

namespace StockMarket.IntegrationTests.Persistence;

public sealed class WatchlistRepositoryTests :
    IClassFixture<SqlServerFixture>,
    IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;

    public WatchlistRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    // Runs before each test in this class.
    public async Task InitializeAsync()
    {
        await using var db = _fixture.CreateDbContext();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetWatchlistByIdAsync_WhenWatchlistExists_LoadsItems()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();

        var company = new Company("Microsoft Corporation", "US");

        var listing = new StockListing(
            company.CompanyId,
            new TickerSymbol("MSFT"),
            new ExchangeCode("XNAS"),
            new CurrencyCode("USD"));

        var watchlist = new Watchlist(userId, "Tech Stocks");

        watchlist.AddStock(listing.StockListingId);

        // Save the initial entities.
        await using (var db = _fixture.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                NormalizedUserName = $"USER-{userId:N}".ToUpperInvariant()
            });

            db.Companies.Add(company);
            db.StockListings.Add(listing);

            var repository = new WatchlistRepository(db);

            await repository.AddAsync(watchlist);
            await db.SaveChangesAsync();
        }

        // Act: create a completely new DbContext.
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);

            var actual = await repository.GetWatchlistByIdAsync(watchlist.WatchlistId);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(watchlist.WatchlistId, actual.WatchlistId);
            Assert.Equal("Tech Stocks", actual.Name);
            var item = Assert.Single(actual.Items);
            Assert.Equal(listing.StockListingId, item.StockListingId);

            // Verify the aggregate's loaded items are
            // available to enforce its business rules.
            Assert.Throws<InvalidOperationException>(
                () => actual.AddStock(listing.StockListingId));
        }
    }


    [Fact]
    public async Task RemoveStock_WhenSaved_DeletesWatchlistItem()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();

        var company = new Company("Apple Inc.", "US");

        var listing = new StockListing(
            company.CompanyId,
            new TickerSymbol("AAPL"),
            new ExchangeCode("XNAS"),
            new CurrencyCode("USD"));

        var watchlist = new Watchlist(userId, "Tech Stocks");

        watchlist.AddStock(listing.StockListingId);

        await using (var db = _fixture.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                NormalizedUserName = $"USER-{userId:N}".ToUpperInvariant()
            });

            db.Companies.Add(company);
            db.StockListings.Add(listing);
            db.Watchlists.Add(watchlist);

            await db.SaveChangesAsync();
        }

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);

            var actual = await repository
                .GetWatchlistByIdAsync(watchlist.WatchlistId);

            Assert.NotNull(actual);

            actual.RemoveStock(listing.StockListingId);

            await db.SaveChangesAsync();
        }

        // Assert: verify using another fresh DbContext.
        await using (var db = _fixture.CreateDbContext())
        {
            int itemCount = await db.WatchlistItems
                .CountAsync(x => x.WatchlistId == watchlist.WatchlistId);

            Assert.Equal(0, itemCount);
        }
    }

    [Fact]
    public async Task GetWatchlistByIdAsync_WhenWatchlistDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentWatchlistId = Guid.CreateVersion7();

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);
            var actual = await repository.GetWatchlistByIdAsync(nonExistentWatchlistId);

            // Assert
            Assert.Null(actual);
        }
    }

    [Fact]
    public async Task WatchlistExistsByNameAsync_WhenWatchlistExists_ReturnsTrue()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();
        var watchlistName = "Tech Stocks";

        var watchlist = new Watchlist(userId, watchlistName);

        await using (var db = _fixture.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                NormalizedUserName = $"USER-{userId:N}".ToUpperInvariant()
            });

            var repository = new WatchlistRepository(db);
            await repository.AddAsync(watchlist);
            await db.SaveChangesAsync();
        }

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);
            var exists = await repository.WatchlistExistsByNameAsync(userId, watchlistName);

            // Assert
            Assert.True(exists);
        }
    }

    [Fact]
    public async Task WatchlistExistsByNameAsync_WhenWatchlistNotExists_ReturnsFalse()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();

        await using (var db = _fixture.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                NormalizedUserName = $"USER-{userId:N}".ToUpperInvariant()
            });

            await db.SaveChangesAsync();
        }

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);
            var exists = await repository.WatchlistExistsByNameAsync(userId, "NonExistent");

            // Assert
            Assert.False(exists);
        }
    }

    [Fact]
    public async Task WatchlistExistsByNameAsync_WhenSameNameDifferentUsers_ReturnsIndividualResults()
    {
        // Arrange
        Guid userAId = Guid.CreateVersion7();
        Guid userBId = Guid.CreateVersion7();
        var watchlistName = "Tech Stocks";

        var watchlistA = new Watchlist(userAId, watchlistName);
        var watchlistB = new Watchlist(userBId, watchlistName);

        await using (var db = _fixture.CreateDbContext())
        {
            db.Users.AddRange(
                new ApplicationUser
                {
                    Id = userAId,
                    UserName = $"user-{userAId:N}",
                    NormalizedUserName = $"USER-{userAId:N}".ToUpperInvariant()
                },
                new ApplicationUser
                {
                    Id = userBId,
                    UserName = $"user-{userBId:N}",
                    NormalizedUserName = $"USER-{userBId:N}".ToUpperInvariant()
                });

            var repository = new WatchlistRepository(db);
            await repository.AddAsync(watchlistA);
            await repository.AddAsync(watchlistB);
            await db.SaveChangesAsync();
        }

        // Act & Assert
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);

            // User A should find their watchlist
            var existsForUserA = await repository.WatchlistExistsByNameAsync(userAId, watchlistName);
            Assert.True(existsForUserA);

            // User B should also find their watchlist
            var existsForUserB = await repository.WatchlistExistsByNameAsync(userBId, watchlistName);
            Assert.True(existsForUserB);

            // User C should not find the watchlist
            Guid userCId = Guid.CreateVersion7();
            var existsForUserC = await repository.WatchlistExistsByNameAsync(userCId, watchlistName);
            Assert.False(existsForUserC);
        }
    }

    [Fact]
    public async Task AddAsync_WhenMultipleWatchlistsForSameUser_PersistsAll()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();
        var watchlist1 = new Watchlist(userId, "Tech Stocks");
        var watchlist2 = new Watchlist(userId, "Financial Stocks");
        var watchlist3 = new Watchlist(userId, "Energy Stocks");

        await using (var db = _fixture.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                NormalizedUserName = $"USER-{userId:N}".ToUpperInvariant()
            });

            var repository = new WatchlistRepository(db);
            await repository.AddAsync(watchlist1);
            await repository.AddAsync(watchlist2);
            await repository.AddAsync(watchlist3);
            await db.SaveChangesAsync();
        }

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            int count = await db.Watchlists
                .CountAsync(x => x.UserId == userId);

            // Assert
            Assert.Equal(3, count);
            var watchlists = await db.Watchlists
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.Name)
                .ToListAsync();

            Assert.Collection(watchlists,
                w => Assert.Equal("Energy Stocks", w.Name),
                w => Assert.Equal("Financial Stocks", w.Name),
                w => Assert.Equal("Tech Stocks", w.Name));
        }
    }

    [Fact]
    public async Task AddAsync_WithConcurrentInserts_AllPersist()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();
        var watchlistIds = new List<Guid>();

        await using (var db = _fixture.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                NormalizedUserName = $"USER-{userId:N}".ToUpperInvariant()
            });

            await db.SaveChangesAsync();
        }

        // Act: add multiple watchlists concurrently
        var tasks = Enumerable.Range(1, 5)
            .Select(async i =>
            {
                await using var db = _fixture.CreateDbContext();
                var watchlist = new Watchlist(userId, $"Watchlist {i}");
                watchlistIds.Add(watchlist.WatchlistId);

                var repository = new WatchlistRepository(db);
                await repository.AddAsync(watchlist);
                await db.SaveChangesAsync();
            })
            .ToList();

        await Task.WhenAll(tasks);

        // Assert: verify all watchlists persisted
        await using (var db = _fixture.CreateDbContext())
        {
            int count = await db.Watchlists
                .CountAsync(x => x.UserId == userId);

            Assert.Equal(5, count);

            var persistedIds = await db.Watchlists
                .Where(x => x.UserId == userId)
                .Select(x => x.WatchlistId)
                .ToListAsync();

            foreach (var id in watchlistIds)
            {
                Assert.Contains(id, persistedIds);
            }
        }
    }

    [Fact]
    public async Task RemoveStock_WhenStockNotExists_ThrowsInvalidOperationException()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();
        var watchlist = new Watchlist(userId, "Tech Stocks");
        var nonExistentStockId = Guid.CreateVersion7();

        await using (var db = _fixture.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                NormalizedUserName = $"USER-{userId:N}".ToUpperInvariant()
            });

            db.Watchlists.Add(watchlist);
            await db.SaveChangesAsync();
        }

        // Act & Assert
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);
            var actual = await repository.GetWatchlistByIdAsync(watchlist.WatchlistId);

            Assert.NotNull(actual);
            Assert.Throws<InvalidOperationException>(
                () => actual.RemoveStock(nonExistentStockId));
        }
    }

    [Fact]
    public async Task Rename_WhenValidName_UpdatesAndPersists()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();
        var watchlist = new Watchlist(userId, "Tech Stocks");
        var newName = "Technology Companies";

        await using (var db = _fixture.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                NormalizedUserName = $"USER-{userId:N}".ToUpperInvariant()
            });

            db.Watchlists.Add(watchlist);
            await db.SaveChangesAsync();
        }

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);
            var actual = await repository.GetWatchlistByIdAsync(watchlist.WatchlistId);

            Assert.NotNull(actual);
            actual.Rename(newName);
            await db.SaveChangesAsync();
        }

        // Assert: verify rename persisted with new DbContext
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);
            var actual = await repository.GetWatchlistByIdAsync(watchlist.WatchlistId);

            Assert.NotNull(actual);
            Assert.Equal(newName, actual.Name);
        }
    }

    [Fact]
    public async Task AddStock_WhenDuplicateStock_ThrowsInvalidOperationException()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();
        var company = new Company("Apple Inc.", "US");
        var listing = new StockListing(
            company.CompanyId,
            new TickerSymbol("AAPL"),
            new ExchangeCode("XNAS"),
            new CurrencyCode("USD"));

        var watchlist = new Watchlist(userId, "Tech Stocks");

        await using (var db = _fixture.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                NormalizedUserName = $"USER-{userId:N}".ToUpperInvariant()
            });

            db.Companies.Add(company);
            db.StockListings.Add(listing);
            db.Watchlists.Add(watchlist);

            await db.SaveChangesAsync();
        }

        // Act & Assert
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);
            var actual = await repository.GetWatchlistByIdAsync(watchlist.WatchlistId);

            Assert.NotNull(actual);

            // Add stock first time - should succeed
            actual.AddStock(listing.StockListingId);

            // Add same stock second time - should throw
            Assert.Throws<InvalidOperationException>(
                () => actual.AddStock(listing.StockListingId));
        }
    }

    [Fact]
    public async Task GetWatchlistByIdAsync_PreservesAllAggregateInvariants_AfterModifications()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();
        var company1 = new Company("Microsoft Corporation", "US");
        var company2 = new Company("Apple Inc.", "US");

        var listing1 = new StockListing(
            company1.CompanyId,
            new TickerSymbol("MSFT"),
            new ExchangeCode("XNAS"),
            new CurrencyCode("USD"));

        var listing2 = new StockListing(
            company2.CompanyId,
            new TickerSymbol("AAPL"),
            new ExchangeCode("XNAS"),
            new CurrencyCode("USD"));

        var watchlist = new Watchlist(userId, "Tech Stocks");
        watchlist.AddStock(listing1.StockListingId);
        watchlist.AddStock(listing2.StockListingId);

        await using (var db = _fixture.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                NormalizedUserName = $"USER-{userId:N}".ToUpperInvariant()
            });

            db.Companies.AddRange(company1, company2);
            db.StockListings.AddRange(listing1, listing2);
            db.Watchlists.Add(watchlist);

            await db.SaveChangesAsync();
        }

        // Act: perform modifications in a new context
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);
            var actual = await repository.GetWatchlistByIdAsync(watchlist.WatchlistId);

            Assert.NotNull(actual);

            // Remove a stock
            actual.RemoveStock(listing2.StockListingId);

            // Rename watchlist
            actual.Rename("Updated Tech Stocks");

            await db.SaveChangesAsync();
        }

        // Assert: verify all modifications persisted and aggregate is valid
        await using (var db = _fixture.CreateDbContext())
        {
            var repository = new WatchlistRepository(db);
            var actual = await repository.GetWatchlistByIdAsync(watchlist.WatchlistId);

            Assert.NotNull(actual);
            Assert.Equal("Updated Tech Stocks", actual.Name);
            Assert.Single(actual.Items);
            Assert.Equal(listing1.StockListingId, actual.Items.First().StockListingId);

            // Verify aggregate invariants still enforced
            Assert.Throws<InvalidOperationException>(
                () => actual.AddStock(listing1.StockListingId));
        }
    }

}
