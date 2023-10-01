using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Repositories.Interfaces;

public interface IUserActionTokenRepository : IBaseRepository<UserActionToken>
{
    public Task<UserActionToken?> FindByTokenValueAndTypeAsync(string tokenValue, ActionTokenTypeEnum tokenType);
    public Task<UserActionToken?> FindByUserIdAndTokenTypeAsync(string userId, ActionTokenTypeEnum tokenType);
    public Task<bool> IsTokenValueExistsAsync(string tokenValue);
    public Task UpsertAsync(UserActionToken token);
}
