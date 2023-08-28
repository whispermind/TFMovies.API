using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data.Repository.Interfaces;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    public Task<RefreshToken?> FindByTokenAndIpAsync(string token, string ipAddress);
    public Task<RefreshToken?> FindByTokenAsync(string token);
}
