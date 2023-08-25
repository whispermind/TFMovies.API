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

        //User Entity
        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.Nickname)
                  .IsRequired();            
        });


        //UserSecretToken Entity
        builder.Entity<UserActionToken>(entity => {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.UserId, e.TokenType })
                  .HasDatabaseName("UX_UserActionToken_UserId_TokenType")
                  .IsUnique();           

            entity.Property(e => e.TokenType)
                  .IsRequired()
                  .HasConversion<string>();

            entity.Property(e => e.Token)
                 .IsRequired();

            entity.Property(e => e.Expires)
                  .IsRequired();

            entity.Property(e => e.Created)
                  .IsRequired();

            entity.Property(e => e.IsUsed)
                  .HasDefaultValue(false);            

            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserActionTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        //JwrRefreshToken Entity
        builder.Entity<RefreshToken>(entity => {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                  .IsRequired();

            entity.Property(e => e.Expires)
                  .IsRequired();

            entity.Property(e => e.Created)
                  .IsRequired();

            entity.Property(e => e.CreatedByIp)
                 .IsRequired();

            entity.Property(e => e.Revoked);         

            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
