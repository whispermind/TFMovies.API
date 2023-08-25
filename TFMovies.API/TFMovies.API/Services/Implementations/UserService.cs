using Microsoft.Extensions.Options;
using System.Net;
using TFMovies.API.Common.Constants;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;
using TFMovies.API.Exceptions;
using TFMovies.API.Integrations;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;
using TFMovies.API.Utils;

namespace TFMovies.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserActionTokenRepository _actionTokenRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly UserActionTokenSettings _actionTokenSettings;

    public UserService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IUserActionTokenRepository actionTokenRepository,
        IOptions<UserActionTokenSettings> actionTokenSettings,
        IRefreshTokenRepository refreshTokenRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _actionTokenRepository = actionTokenRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _actionTokenSettings = actionTokenSettings.Value;
        _emailService = emailService;
    }
    public async ValueTask<JwtTokensResponse> LoginAsync(LoginRequest model, string callBackUrl, string ipAddress)
    {
        var email = model.Email.Trim().ToLower();

        var userDb = await _userRepository.FindByEmailAsync(email);

        if (userDb == null)
        {
            throw new KeyNotFoundException(ErrorMessages.UserNotFound);
        }

        var isPasswordValid = await _userRepository.CheckPasswordAsync(userDb, model.Password);

        if (!isPasswordValid)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.IncorrectPassword);
        }

        if (!userDb.EmailConfirmed)
        {
            await SendEmailWithActionTokenAsync(userDb, ActionTokenTypeEnum.EmailVerify, callBackUrl);

            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.UnconfirmedEmail);
        }

        var userRoles = await _userRepository.GetRolesAsync(userDb);        

        var accessToken = _jwtService.GenerateAccessToken(userDb, userRoles);

        var refreshToken = _jwtService.GenerateRefreshTokenAsync();

        refreshToken.UserId = userDb.Id;
        refreshToken.CreatedByIp = ipAddress;

        await _refreshTokenRepository.CreateAsync(refreshToken);

        var result = new JwtTokensResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };

        return result;
    }

    public async ValueTask<JwtTokensResponse> RefreshJwtTokens(RefreshTokenRequest model, string ipAddress)
    {
        var tokenDb = await _refreshTokenRepository.FindByTokenAndIpAsync(model.RefreshToken, ipAddress);
        
        if (tokenDb == null || !tokenDb.IsActive)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.InvalidToken);
        }

        var userDb = await _userRepository.FindByIdAsync(tokenDb.UserId);

        var userRoles = await _userRepository.GetRolesAsync(userDb);

        var newAccessToken = _jwtService.GenerateAccessToken(userDb, userRoles);        

        var newRefreshToken = _jwtService.GenerateRefreshTokenAsync();

        if (await _refreshTokenRepository.FindByTokenAsync(newRefreshToken.Token) != null)
        {
            newRefreshToken = _jwtService.GenerateRefreshTokenAsync();
        }

        tokenDb.Created = newRefreshToken.Created;        
        tokenDb.Expires = newRefreshToken.Expires;
        tokenDb.Token = newRefreshToken.Token;

        await _refreshTokenRepository.UpdateAsync(tokenDb);        

        var result = new JwtTokensResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token
        };

        return result;
    }

    public async Task RegisterAsync(RegisterRequest model, string callBackUrl)
    {
        var email = model.Email.Trim().ToLower();
        var userDb = await _userRepository.FindByEmailAsync(email);

        if (userDb != null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.UserAlreadyExists);
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
            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed);
        }

        result = await _userRepository.AddToRoleAsync(userDb, UserRoleEnum.User.ToString());
        if (!result.Succeeded)
        {
            await _userRepository.DeleteAsync(userDb);

            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed);
        }

        await SendEmailWithActionTokenAsync(userDb, ActionTokenTypeEnum.EmailVerify, callBackUrl);
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest model)
    {
        var actionTokenDb = await _actionTokenRepository.FindByTokenValueAndTypeAsync(model.Token, ActionTokenTypeEnum.EmailVerify);        

        if (actionTokenDb == null || !actionTokenDb.IsActive) 
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.InvalidToken);
        }
        var linkedUser = await _userRepository.FindByIdAsync(actionTokenDb.UserId);

        if (linkedUser == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.InvalidToken);
        }

        if (linkedUser.EmailConfirmed)
        {
            return;
        }

        actionTokenDb.IsUsed = true;

        await _actionTokenRepository.UpdateAsync(actionTokenDb);        

        linkedUser.EmailConfirmed = true;

        var result = await _userRepository.UpdateAsync(linkedUser);

        if (!result.Succeeded)
        {
            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed);
        }
    }

    public async Task SendActivationEmailAsync(ActivateEmailRequest model, string callBackUrl)
    {
        var email = model.Email.Trim().ToLower();

        var userDb = await _userRepository.FindByEmailAsync(email);

        if (userDb == null)
        {
            throw new KeyNotFoundException(ErrorMessages.UserNotFound);
        }

        await SendEmailWithActionTokenAsync(userDb, ActionTokenTypeEnum.EmailVerify, callBackUrl);
    }
    public async Task ForgotPasswordAsync(ForgotPasswordRequest model, string callBackUrl)
    {
        var email = model.Email.Trim().ToLower();

        var userDb = await _userRepository.FindByEmailAsync(email);

        if (userDb == null)
        {
            throw new KeyNotFoundException(ErrorMessages.UserNotFound);
        }

        await SendEmailWithActionTokenAsync(userDb, ActionTokenTypeEnum.PasswordReset, callBackUrl);
    }
    
    public async Task ValidateResetTokenAsync(string token, bool setUsed)
    {
        var actionTokenDb = await _actionTokenRepository.FindByTokenValueAndTypeAsync(token, ActionTokenTypeEnum.PasswordReset);

        if (actionTokenDb == null || !actionTokenDb.IsActive)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.InvalidToken);
        }       

        if (setUsed)
        {
            actionTokenDb.IsUsed = true;

            await _actionTokenRepository.UpdateAsync(actionTokenDb);
        }                
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest model)
    {
        await ValidateResetTokenAsync(model.Token, true);

        var email = model.Email.Trim().ToLower();

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
            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed);
        }
    }

    private async Task SendEmailWithActionTokenAsync(User user, ActionTokenTypeEnum tokenType, string callBackUrl)
    {
        var actionToken = await UpsertUserTokenAsync(user.Id, tokenType);

        var link = $"{callBackUrl}?token={actionToken.Token}?email={user.Email}";

        var tokenSettings = GetTokenSettings(actionToken.TokenType);

        var tokenDuration = $"{tokenSettings.Value} {TimeUnitEnumToFriendlyString(tokenSettings.Unit, tokenSettings.Value)}";

        string emailTemplate;
        string emailSubject;

        if (actionToken.TokenType == ActionTokenTypeEnum.PasswordReset)
        {
            emailTemplate = EmailTemplates.PasswordResetBody;
            emailSubject = EmailTemplates.PasswordResetSubject;
        }
        else
        {
            emailTemplate = EmailTemplates.EmailVerifyBody;
            emailSubject = EmailTemplates.EmailVerifySubject;
        }        

        var emailContent = string.Format(emailTemplate, user.Nickname, link, tokenDuration);        

        await _emailService.SendEmailAsync(user.Email, emailSubject, emailContent);
    }
    
    private static string TimeUnitEnumToFriendlyString(TimeUnitEnum unit, int duration)
    {
        string baseStr = unit.ToString();

        if (duration == 1)
        {
            return baseStr.TrimEnd('s');
        }

        return baseStr;
    }
    private async Task<UserActionToken> UpsertUserTokenAsync(string userId, ActionTokenTypeEnum tokenType)
    {
        var token = await GenerateActionTokenAsync();

        var created = DateTime.UtcNow;

        var tokenSettings = GetTokenSettings(tokenType);

        var expires = TimeUtility.AddTime(created, tokenSettings.Unit, tokenSettings.Value);

        var tokenDb = await _actionTokenRepository.FindByUserIdAndTokenTypeAsync(userId, tokenType);

        if (tokenDb == null)
        {
            tokenDb = new UserActionToken
            {
                UserId = userId,
                Token = token,
                TokenType = tokenType,
                Created = created,
                Expires = expires
            };

            await _actionTokenRepository.CreateAsync(tokenDb);
        }
        else
        {
            tokenDb.Token = token;
            tokenDb.Expires = expires;
            tokenDb.Created = created;

            await _actionTokenRepository.UpdateAsync(tokenDb);
        }

        return tokenDb;
    }
    private async Task<string> GenerateActionTokenAsync()
    {
        var token = Guid.NewGuid().ToString();

        var tokenIsUnique = !await _actionTokenRepository.HasTokenAsync(token);
        if (!tokenIsUnique)
        {
            return await GenerateActionTokenAsync();
        }

        return token;
    }
    private TokenDurationSettings GetTokenSettings(ActionTokenTypeEnum tokenType)
    {
        return tokenType switch
        {
            ActionTokenTypeEnum.EmailVerify => _actionTokenSettings.EmailVerifyDuration,
            ActionTokenTypeEnum.PasswordReset => _actionTokenSettings.PasswordResetDuration,
            _ => throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed)
        };
    }    
}
