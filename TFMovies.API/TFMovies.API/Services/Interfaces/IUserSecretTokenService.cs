using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Services.Interfaces;

public interface IUserSecretTokenService
{
    public ValueTask<string> CheckSecretTokenAsync(string token, SecretTokenTypeEnum tokenType);
    public Task<string> CreateAndStoreConfirmEmailTokenAsync(string userId);
    public Task<string> CreateAndStorePasswordRecoveryTokenAsync(string userId);
}
