using Microsoft.Extensions.Options;
using TFMovies.API.Common.Constants;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Services.Interfaces;
using TFMovies.API.Utils;

namespace TFMovies.API.Services.Implementations;

public class UserSecretTokenService : IUserSecretTokenService
{
    private readonly IUserSecretTokenRepository _userSecretTokenRepository;
    private readonly ConfirmEmailTokenSettings _confirmEmailTokenSettings;
    private readonly PasswordResetTokenSettings _passwordResetTokenSettings;

    public UserSecretTokenService(IUserSecretTokenRepository userTokenRepository, IOptions<PasswordResetTokenSettings> passwordResetTokenSettings, IOptions<ConfirmEmailTokenSettings> confirmEmailTokenSettings)
    {
        _userSecretTokenRepository = userTokenRepository;
        _passwordResetTokenSettings = passwordResetTokenSettings.Value;
        _confirmEmailTokenSettings = confirmEmailTokenSettings.Value;
    }

    public async ValueTask<string> CheckSecretTokenAsync(string token, SecretTokenTypeEnum tokenType)
    {
        var userTokenDb = await _userSecretTokenRepository.FindValidByTypeAndValueAsync(token, tokenType);

        if (userTokenDb == null)
        {
            throw new BadRequestException(ErrorMessages.InvalidToken);
        }
        
        userTokenDb.IsUsed = true;
        await _userSecretTokenRepository.UpdateAsync(userTokenDb);
        

        return userTokenDb.UserId;
    }

    public async Task<string> CreateAndStoreConfirmEmailTokenAsync(string userId)
    {
        var token = Guid.NewGuid().ToString();
        var expiryAt = TimeUtility.AddTime(DateTime.UtcNow, _confirmEmailTokenSettings.LifeTimeUnit, _confirmEmailTokenSettings.LifeTimeDuration);

        await StoreTokenIntoDbAsync(userId, token, SecretTokenTypeEnum.ConfirmEmail, expiryAt);

        return token;
    }

    public async Task<string> CreateAndStorePasswordRecoveryTokenAsync(string userId)
    {
        var token = Guid.NewGuid().ToString();
        var expiryAt = TimeUtility.AddTime(DateTime.UtcNow, _passwordResetTokenSettings.LifeTimeUnit, _passwordResetTokenSettings.LifeTimeDuration);

        await StoreTokenIntoDbAsync(userId, token, SecretTokenTypeEnum.PasswordReset, expiryAt);

        return token;
    }
    private async Task StoreTokenIntoDbAsync(string userId, string token, SecretTokenTypeEnum tokenType, DateTime expiryAt)
    {
        var userTokenDb = new UserSecretToken
        {
            UserId = userId,
            Token = token,
            TokenType = tokenType,
            ExpiryAt = expiryAt
        };

        var result = await _userSecretTokenRepository.CreateAsync(userTokenDb);

        if (result == null)
        {
            throw new InternalServerException(ErrorMessages.OperationFailed, new Exception(string.Format(ErrorMessages.OperationFailedDetails, "Storing the token into the DB.")));
        }
    }
}
