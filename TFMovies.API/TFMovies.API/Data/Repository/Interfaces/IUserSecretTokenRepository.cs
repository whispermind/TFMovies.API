using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data.Repository.Interfaces;

public interface IUserSecretTokenRepository : IBaseRepository<UserSecretToken>
{
    public ValueTask<UserSecretToken> FindValidByTypeAndValueAsync(string token, SecretTokenTypeEnum tokenType);
}
