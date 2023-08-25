using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Services.Interfaces;

public interface IUserSecretTokenService
{
    public Task<UserSecretToken> FindByTokenValueAndTypeAsync(string token, SecretTokenTypeEnum tokenType);
    public Task ValidateAndConsumeSecretTokenAsync(UserSecretToken secretToken, bool setIsUsed);
    public Task<UserSecretToken> GenerateAndStoreSecretTokenAsync(string userId, SecretTokenTypeEnum tokenType);
}
