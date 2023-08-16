using System.Security.Claims;
using TFMovies.API.Common.Constants;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IJwtService _jwtService;
    private readonly IUserSecretTokenService _userSecretTokenService;
    private readonly IEmailTemplateService _emailTemplateService;

    public UserService(IUserRepository userRepository, IEmailService emailService, IJwtService jwtService, IUserSecretTokenService userSecretTokenService, IEmailTemplateService emailTemplateService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _jwtService = jwtService;
        _userSecretTokenService = userSecretTokenService;
        _emailTemplateService = emailTemplateService;
    }
    #region Registration
    public async Task RegisterAsync(UserRegisterRequest model, string callBackUrl)
    {
        var email = model.Email.ToLower();
        var userDb = await _userRepository.FindByEmailAsync(email);

        if (userDb != null)
        {
            throw new ConflictException(ErrorMessages.UserAlreadyExists);
        }

        userDb =await CreateUserAndAssignRole(model, email);

        await SendConfirmationEmail(userDb, callBackUrl);
    }
    #endregion

    #region Login
    public async ValueTask<JwtTokensResponse> LoginAsync(UserLoginRequest model)
    {
        var userDb = await _userRepository.FindByEmailAsync(model.Email);
        if (userDb == null)
        {
            throw new KeyNotFoundException(ErrorMessages.UserNotFound);
        }

        var isPasswordValid = await _userRepository.CheckPasswordAsync(userDb, model.Password);
        if (!isPasswordValid)
        {
            throw new BadRequestException(ErrorMessages.IncorrecrPassword);
        }

        var claims = new List<Claim>
        {
            new Claim("sub", userDb.Id),
            new Claim("email", userDb.Email)
        };

        var userRoles = await _userRepository.GetRolesAsync(userDb);

        if (!userRoles.Any())
        {
            throw new InternalServerException(ErrorMessages.OperationFailed, new Exception($"{string.Format(ErrorMessages.OperationFailedDetails, "AccessToken generation")}: {ErrorMessages.UserWithoutRoles}"));
        }

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
        }

        var accessToken = _jwtService.GenerateAccessToken(claims);

        var refeshToken = await _jwtService.GenerateRefreshToken(userDb.Id);

        var result = new JwtTokensResponse
        {
            AccessToken = accessToken,
            RefreshToken = refeshToken
        };

        return result;
    }

    #endregion

    #region ConfirmEmailByTokenAsync
    public async Task ConfirmEmailByTokenAsync(string token)
    {
        var userId = await _userSecretTokenService.CheckSecretTokenAsync(token, SecretTokenTypeEnum.ConfirmEmail);

        var user = await _userRepository.FindByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException(ErrorMessages.UserNotFound);
        }

        user.EmailConfirmed = true;
        await _userRepository.UpdateAsync(user);
    }
    #endregion

    # region RequestNewConfirmationEmailAsync
    public async Task RequestNewConfirmationEmailAsync(string email, string callBackUrl)
    {
        email = email.ToLower();

        var userDb = await _userRepository.FindByEmailAsync(email);

        if (userDb == null)
        {
            throw new KeyNotFoundException(ErrorMessages.UserNotFound);
        }

        await SendConfirmationEmail(userDb, callBackUrl);
    }
    #endregion

    #region private methods
    private async ValueTask<User> CreateUserAndAssignRole(UserRegisterRequest model, string email)
    {
        email = email.ToLower();

        var userDb = new User
        {
            UserName = email,
            Email = email,
            Nickname = model.Nickname
        };

        var result = await _userRepository.CreateAsync(userDb, model.Password);
        if (!result.Succeeded)
        {
            throw new InternalServerException(ErrorMessages.OperationFailed, new Exception(string.Format(ErrorMessages.OperationFailedDetails, "User registration - User creation in Db")));
        }

        result = await _userRepository.AddToRoleAsync(userDb, UserRoleEnum.User.ToString());
        if (!result.Succeeded)
        {
            await _userRepository.DeleteAsync(userDb);

            throw new InternalServerException(ErrorMessages.OperationFailed, new Exception(string.Format(ErrorMessages.OperationFailedDetails, "User registration - adding to Role")));
        }

        return userDb;
    }

    private async Task SendConfirmationEmail(User user, string callBackUrl)
    {
        var token = await _userSecretTokenService.CreateAndStoreConfirmEmailTokenAsync(user.Id);
        var emailContent = _emailTemplateService.GetConfirmEmailContent(user, callBackUrl, token);
        await _emailService.SendEmailAsync(user.Email, "Confirm Your Email", emailContent);
    }
    #endregion
}
