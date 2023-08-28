using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data;

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<UserActionToken> UserActionTokens { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserActionToken>()
        .HasIndex(e => new { e.UserId, e.TokenType })
        .HasDatabaseName("UX_UserActionToken_UserId_TokenType")
        .IsUnique();
    }
}
