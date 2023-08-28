using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data.Repository.Interfaces;

public interface IUserSecretTokenRepository : IBaseRepository<UserSecretToken>
{
    public Task<UserSecretToken?> FindByTokenValueAndTypeAsync(string token, SecretTokenTypeEnum tokenType);
    public Task<UserSecretToken?> FindByUserIdAndTokenTypeAsync(string userId, SecretTokenTypeEnum tokenType);
}
