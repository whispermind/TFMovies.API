using Microsoft.EntityFrameworkCore;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(DataContext context) : base(context)
    { }

    public async Task<RefreshToken?> FindByTokenAndIpAsync(string token, string ipAddress)
    {
        var result = await _entities
               .FirstOrDefaultAsync(item =>
                 item.Token == token
                 && item.CreatedByIp == ipAddress);                 

        return result;
    }

    public async Task<RefreshToken?> FindByTokenAsync(string token)
    {
        var result = await _entities
               .FirstOrDefaultAsync(item =>
                 item.Token == token);

        return result;
    }
}
