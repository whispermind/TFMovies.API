using Microsoft.EntityFrameworkCore;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public class JwtRefreshTokenRepository : BaseRepository<JwtRefreshToken>, IJwtRefreshTokenRepository
{
    public JwtRefreshTokenRepository(DataContext context) : base(context)
    { }

    public async Task<JwtRefreshToken?> GetActiveTokenAsync(string token)
    {
        var result = await _entities
               .FirstOrDefaultAsync(item =>
                 item.Token == token   
                 && item.ExpiryAt > DateTime.UtcNow
                 && !item.IsUsed
                 && !item.IsRevoked);

        return result;
    }
}
