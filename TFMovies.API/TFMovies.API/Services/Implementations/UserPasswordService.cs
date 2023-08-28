using TFMovies.API.Common.Constants;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Repository.Interfaces;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Requests;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Services.Implementations;

public class UserPasswordService : IUserPasswordService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSecretTokenService _userSecretTokenService;
    private readonly IUserEmailService _userEmailService;    

    public UserPasswordService(
        IUserRepository userRepository, 
        IUserSecretTokenService userSecretTokenService,
        IUserEmailService userEmailService)
    {
        _userRepository = userRepository;
        _userSecretTokenService = userSecretTokenService;
        _userEmailService = userEmailService;        
    }
    public async Task SendResetPasswordLinkToEmailAsync(string email, string callBackUrl)
    {
        email = email.ToLower();

        var userDb = await _userRepository.FindByEmailAsync(email);

        if (userDb == null)
        {
            throw new KeyNotFoundException(ErrorMessages.UserNotFound);
        }

        await _userEmailService.SendPasswordRecoveryEmailAsync(userDb, callBackUrl);
    }

    public async Task VerifyResetTokenAsync(string token, bool setIsUsed)
    {
        var secretTokenDb = await _userSecretTokenService.FindByTokenValueAndTypeAsync(token, SecretTokenTypeEnum.ResetPassword);  //return InvalidToken if not found         

        await _userSecretTokenService.ValidateAndConsumeSecretTokenAsync(secretTokenDb, setIsUsed); //return InvalidToken if isUsed=true or ExpiryAt>DateTime.UtcNow        
    }

    public async Task RecoveryPasswordAsync(RecoveryPasswordRequest model)
    {
        await VerifyResetTokenAsync(model.Token, true);

        var email = model.Email.ToLower();

        var userDb = await _userRepository.FindByEmailAsync(email);

        if (userDb == null)
        {
            throw new KeyNotFoundException(ErrorMessages.UserNotFound);
        }        

        var hashedPassword = _userRepository.HashPassword(userDb, model.NewPassword);

        userDb.PasswordHash = hashedPassword;

        var result = await _userRepository.UpdateAsync(userDb);

        if (!result.Succeeded)
        {
            throw new InternalServerException(ErrorMessages.OperationFailed, 
                new Exception(string.Format(ErrorMessages.OperationFailedDetails, "Reset Password - updating Password in Db")));
        }
    }    
}
