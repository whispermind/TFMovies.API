using Microsoft.EntityFrameworkCore;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public class UserActionTokenRepository : BaseRepository<UserActionToken>, IUserActionTokenRepository
{
    public UserActionTokenRepository(DataContext context) : base(context)
    { }

    public async Task<UserActionToken?> FindByTokenValueAndTypeAsync(string token, ActionTokenTypeEnum tokenType)
    {
        var result = await _entities
             .FirstOrDefaultAsync(item =>
                 item.Token == token
                 && item.TokenType == tokenType);

        return result;
    }

    public async Task<UserActionToken?> FindByUserIdAndTokenTypeAsync(string userId, ActionTokenTypeEnum tokenType)
    {
        var result = await _entities
             .FirstOrDefaultAsync(item =>
                 item.UserId == userId
                 && item.TokenType == tokenType);

        return result;
    }

    public async Task<bool> HasTokenAsync(string token)
    {
        var result = await _entities
               .AnyAsync(x => x.Token == token);

        return result;
    }
}
