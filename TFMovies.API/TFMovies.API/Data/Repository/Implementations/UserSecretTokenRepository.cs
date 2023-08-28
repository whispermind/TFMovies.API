using Microsoft.EntityFrameworkCore;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public class UserSecretTokenRepository : BaseRepository<UserSecretToken>, IUserSecretTokenRepository
{
    public UserSecretTokenRepository(DataContext context) : base(context)
    { }

    public async Task<UserSecretToken?> FindByTokenValueAndTypeAsync(string token, SecretTokenTypeEnum tokenType)
    {
        var result = await _entities
             .FirstOrDefaultAsync(item =>
                 item.Token == token
                 && item.TokenType == tokenType);

        return result;
    }

    public async Task<UserSecretToken?> FindByUserIdAndTokenTypeAsync(string userId, SecretTokenTypeEnum tokenType)
    {
        var result = await _entities
             .FirstOrDefaultAsync(item =>
                 item.UserId == userId
                 && item.TokenType == tokenType);

        return result;
    }
}
