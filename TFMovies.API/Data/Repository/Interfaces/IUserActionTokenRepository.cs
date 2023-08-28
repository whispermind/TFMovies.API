using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data.Repository.Interfaces;

public interface IUserActionTokenRepository : IBaseRepository<UserActionToken>
{
    public Task<UserActionToken?> FindByTokenValueAndTypeAsync(string token, ActionTokenTypeEnum tokenType);
    public Task<UserActionToken?> FindByUserIdAndTokenTypeAsync(string userId, ActionTokenTypeEnum tokenType);
    public Task<bool> HasTokenAsync(string token);
}
