using TFMovies.API.Common.Constants;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Requests;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Services.Implementations;

public class UserRegistrationService : IUserRegistrationService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSecretTokenService _userSecretTokenService;
    private readonly IUserEmailService _userEmailService;    

    public UserRegistrationService(
        IUserRepository userRepository, 
        IUserSecretTokenService userSecretTokenService, 
        IUserEmailService userEmailService)
    {
        _userRepository = userRepository;
        _userSecretTokenService = userSecretTokenService;
        _userEmailService = userEmailService;        
    }
    public async Task RegisterAsync(UserRegisterRequest model, string callBackUrl)
    {
        var email = model.Email.ToLower();
        var userDb = await _userRepository.FindByEmailAsync(email);

        if (userDb != null)
        {
            throw new ConflictException(ErrorMessages.UserAlreadyExists);
        }

        userDb = new User
        {
            UserName = email,
            Email = email,
            Nickname = model.Nickname
        };

        var result = await _userRepository.CreateAsync(userDb, model.Password);
        if (!result.Succeeded)
        {
            throw new InternalServerException(ErrorMessages.OperationFailed, 
                new Exception(string.Format(ErrorMessages.OperationFailedDetails, "User registration - User creation in Db")));
        }

        result = await _userRepository.AddToRoleAsync(userDb, UserRoleEnum.User.ToString());
        if (!result.Succeeded)
        {
            await _userRepository.DeleteAsync(userDb);

            throw new InternalServerException(ErrorMessages.OperationFailed, 
                new Exception(string.Format(ErrorMessages.OperationFailedDetails, "User registration - adding to Role")));
        }

        await _userEmailService.SendConfirmationLinkAsync(userDb, callBackUrl);
    }

    public async Task ConfirmEmailByTokenAsync(string token)
    {
        var secretTokenDb = await _userSecretTokenService.FindByTokenValueAndTypeAsync(token, SecretTokenTypeEnum.ConfirmEmail);  //return InvalidToken if not found      

        var linkedUser = await _userRepository.FindByIdAsync(secretTokenDb.UserId);
        
        if (linkedUser == null)
        {
            throw new BadRequestException(ErrorMessages.InvalidToken, 
                new Exception(string.Format(ErrorMessages.OperationFailedDetails, "Linked user was not found by Id {secretToken.UserId}")));
        }

        if (linkedUser.EmailConfirmed)
        {
            return;
        }

        await _userSecretTokenService.ValidateAndConsumeSecretTokenAsync(secretTokenDb, true); //return InvalidToken if isUsed=true or ExpiryAt>DateTime.UtcNow

        linkedUser.EmailConfirmed = true;

        var result = await _userRepository.UpdateAsync(linkedUser);
        
        if (!result.Succeeded)
        {
            throw new InternalServerException(ErrorMessages.OperationFailed, 
                new Exception(string.Format(ErrorMessages.OperationFailedDetails, "Email confirmation - updating EmailConfirmed field in Db")));
        }              
    }

    public async Task SendConfirmationLinkToEmailAsync(string email, string callBackUrl)
    {
        email = email.ToLower();

        var userDb = await _userRepository.FindByEmailAsync(email);

        if (userDb == null)
        {
            throw new KeyNotFoundException(ErrorMessages.UserNotFound);
        }

        await _userEmailService.SendConfirmationLinkAsync(userDb, callBackUrl);
    }    
}
