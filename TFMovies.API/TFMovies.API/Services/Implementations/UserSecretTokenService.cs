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
    private readonly ISecretTokenSettings _confirmEmailTokenSettings;
    private readonly ISecretTokenSettings _resetPasswordTokenSettings;

    public UserSecretTokenService(
        IUserSecretTokenRepository userTokenRepository, 
        IOptions<ConfirmEmailTokenSettings> confirmEmailTokenSettings,
        IOptions<ResetPasswordTokenSettings> resetPasswordTokenSettings)
    {
        _userSecretTokenRepository = userTokenRepository;
        _confirmEmailTokenSettings = confirmEmailTokenSettings.Value;
        _resetPasswordTokenSettings = resetPasswordTokenSettings.Value;
    }

    public async Task<UserSecretToken> FindByTokenValueAndTypeAsync(string token, SecretTokenTypeEnum tokenType)
    {
        var userTokenDb = await _userSecretTokenRepository.FindByTokenValueAndTypeAsync(token, tokenType);

        if (userTokenDb == null)
        {
            throw new BadRequestException(ErrorMessages.InvalidToken);
        }

        return userTokenDb;
    }

    public async Task ValidateAndConsumeSecretTokenAsync(UserSecretToken secretToken, bool setIsUsed)
    {
        if (secretToken.IsUsed || secretToken.ExpiryAt < DateTime.UtcNow)
        {
            throw new BadRequestException(ErrorMessages.InvalidToken);
        }

        if (setIsUsed)
        {
            secretToken.IsUsed = true;
        }

        await _userSecretTokenRepository.UpdateAsync(secretToken);        
    }

    public async Task<UserSecretToken> GenerateAndStoreSecretTokenAsync(string userId, SecretTokenTypeEnum tokenType)
    {
        var token = Guid.NewGuid().ToString();

        var createdAt = DateTime.UtcNow;

        var expiryAt = tokenType switch
        {
            SecretTokenTypeEnum.ConfirmEmail => CalculateTokenExpiryAt(createdAt, _confirmEmailTokenSettings),
            SecretTokenTypeEnum.ResetPassword => CalculateTokenExpiryAt(createdAt, _resetPasswordTokenSettings),
            _ => throw new InternalServerException(ErrorMessages.OperationFailed, new Exception(string.Format(ErrorMessages.OperationFailedDetails, "Calculating ExpiryAt for the SecretToken")))
        };        

        var secretTokenDb = await _userSecretTokenRepository.FindByUserIdAndTokenTypeAsync(userId, tokenType);

        UserSecretToken result;

        if (secretTokenDb == null)
        {
            secretTokenDb = new UserSecretToken
            {
                UserId = userId,
                Token = token,
                TokenType = tokenType,
                CreatedAt = createdAt,
                ExpiryAt = expiryAt
            };

           result = await _userSecretTokenRepository.CreateAsync(secretTokenDb);            
        }
        else
        {
            secretTokenDb.Token = token;
            secretTokenDb.ExpiryAt = expiryAt;

           result = await _userSecretTokenRepository.UpdateAsync(secretTokenDb);            
        }

        if (result == null)
        {
            throw new InternalServerException(ErrorMessages.OperationFailed, new Exception(string.Format(ErrorMessages.OperationFailedDetails, "Storing the token into the DB.")));
        }

        return secretTokenDb;
    }

    private DateTime CalculateTokenExpiryAt(DateTime dateTime, ISecretTokenSettings tokenSettings)
    {
        return TimeUtility.AddTime(dateTime, tokenSettings.LifeTimeUnit, tokenSettings.LifeTimeDuration);
    }
}
