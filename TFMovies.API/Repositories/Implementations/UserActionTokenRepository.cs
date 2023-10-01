using Microsoft.EntityFrameworkCore;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data;
using TFMovies.API.Data.Entities;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

public class UserActionTokenRepository : BaseRepository<UserActionToken>, IUserActionTokenRepository
{
    public UserActionTokenRepository(DataContext context) : base(context)
    { }

    public async Task<UserActionToken?> FindByTokenValueAndTypeAsync(string tokenValue, ActionTokenTypeEnum tokenType)
    {
        var result = await _entities
             .FirstOrDefaultAsync(item =>
                 item.Token == tokenValue
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

    public async Task<bool> IsTokenValueExistsAsync(string tokenValue)
    {
        var result = await _entities
               .AnyAsync(x => x.Token == tokenValue);

        return result;
    }

    public async Task UpsertAsync(UserActionToken token)
    {       
        var existingToken = await FindByUserIdAndTokenTypeAsync(token.UserId, token.TokenType);
        
        if (existingToken == null)
        {
            await _entities.AddAsync(token);
        }
        else
        {
            existingToken.Token = token.Token;
            existingToken.ExpiresAt = token.ExpiresAt;
            existingToken.CreatedAt = token.CreatedAt;
            existingToken.IsUsed = false;           
        }

        await SaveChangesAsync();
    }
}
