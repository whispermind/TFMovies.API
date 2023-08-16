using Microsoft.EntityFrameworkCore;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public class JwtRefreshTokenRepository : BaseRepository<JwtRefreshToken>, IJwtRefreshTokenRepository
{
    public JwtRefreshTokenRepository(DataContext context) : base(context)
    { }

    public async ValueTask<JwtRefreshToken> CheckTokenValidAsync(string token, string userId)
    {
        var result = await _entities
               .FirstOrDefaultAsync(item =>
                 item.Token == token
                 && item.UserId == userId
                 && item.ExpiryAt > DateTime.UtcNow
                 && !item.IsUsed
                 && !item.IsRevoked);

        return result;
    }
}
