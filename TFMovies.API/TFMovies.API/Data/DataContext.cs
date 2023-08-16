using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data;

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<UserSecretToken> UserSecretTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //User Entity
        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.Nickname)
                  .IsRequired();            
        });


        //UserSecretToken Entity
        builder.Entity<UserSecretToken>(entity => {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                  .IsRequired();

            entity.Property(e => e.TokenType)
                  .IsRequired()
                  .HasConversion<string>();

            entity.Property(e => e.ExpiryAt)
                  .IsRequired();

            entity.Property(e => e.IsUsed)
                  .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GetUtcDate()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserSecretTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        //JwrRefreshToken Entity
        builder.Entity<JwtRefreshToken>(entity => {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                  .IsRequired();

            entity.Property(e => e.ExpiryAt)
                  .IsRequired();

            entity.Property(e => e.IsUsed)
                  .HasDefaultValue(false);

            entity.Property(e => e.IsRevoked)
                  .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GetUtcDate()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.JwtRefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
