using Microsoft.EntityFrameworkCore;
using TFMovies.API.Data;
using TFMovies.API.Data.Entities;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(DataContext context) : base(context)
    { }   

    public async Task<RefreshToken?> FindByTokenAsync(string token)
    {
        var result = await _entities
               .FirstOrDefaultAsync(item => item.Token == token);

        return result;
    }
}
