using Microsoft.AspNetCore.Identity;
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
        var userDb = await GetUserOrThrowAsync(email: model.Email);

        var isPasswordValid = await _userRepository.CheckPasswordAsync(userDb, model.Password);

        if (!isPasswordValid)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.IncorrectPassword);
        }

        if (!userDb.EmailConfirmed)
        {
            await SendEmailByEmailSubjectAsync(userDb, EmailTemplates.EmailVerifySubject, callBackUrl);

            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.UnconfirmedEmail);
        }

        var userRoles = await _userRepository.GetRolesAsync(userDb);        

        var accessToken = _jwtService.GenerateAccessToken(userDb, userRoles);

        var refreshToken = await GenerateUniqueRefreshToken();

        refreshToken.UserId = userDb.Id;        
        refreshToken.CreatedByIp = ipAddress;        

        await _refreshTokenRepository.CreateAsync(refreshToken);

        return new JwtTokensResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };        
    }

    public async ValueTask<JwtTokensResponse> RefreshJwtTokens(RefreshTokenRequest model, string ipAddress)
    {
        var tokenDb = await ValidateRefreshToken(model.RefreshToken, ipAddress);

        var userDb = await GetUserOrThrowAsync(userId: tokenDb.UserId);        

        var userRoles = await _userRepository.GetRolesAsync(userDb);

        var newAccessToken = _jwtService.GenerateAccessToken(userDb, userRoles);        

        var newRefreshToken = await GenerateUniqueRefreshToken();       

        await UpdateTokenDbWithNewRefreshToken(tokenDb, newRefreshToken);

        return new JwtTokensResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token
        };        
    }

    public async Task RegisterAsync(RegisterRequest model, string callBackUrl)
    {       
        await GetUserOrThrowAsync(email: model.Email, throwIfUserExists: true);

        var email = model.Email.Trim().ToLower();

        var newUser = new User
        {
            UserName = email,
            Email = email,
            Nickname = model.Nickname
        };

        var result = await _userRepository.CreateAsync(newUser, model.Password);
        
        EnsureSuccess(result);

        result = await _userRepository.AddToRoleAsync(newUser, UserRoleEnum.User.ToString());

        if (!result.Succeeded)
        {
            await _userRepository.DeleteAsync(newUser);

            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed);
        }

        await SendEmailByEmailSubjectAsync(newUser, EmailTemplates.EmailVerifySubject, callBackUrl);
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest model)
    {
        var actionTokenDb = await _actionTokenRepository.FindByTokenValueAndTypeAsync(model.Token, ActionTokenTypeEnum.EmailVerify);        

        if (actionTokenDb == null || !actionTokenDb.IsActive) 
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.InvalidToken);
        }

        var linkedUser = await GetUserOrThrowAsync(userId: actionTokenDb.UserId);        

        if (linkedUser.EmailConfirmed)
        {
            return;
        }

        actionTokenDb.IsUsed = true;

        await _actionTokenRepository.UpdateAsync(actionTokenDb);        

        linkedUser.EmailConfirmed = true;

        var result = await _userRepository.UpdateAsync(linkedUser);

        EnsureSuccess(result);
    }

    public async Task SendActivationEmailAsync(ActivateEmailRequest model, string callBackUrl)
    {
        var userDb = await GetUserOrThrowAsync(email: model.Email);

        await SendEmailByEmailSubjectAsync(userDb, EmailTemplates.EmailVerifySubject, callBackUrl);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest model, string callBackUrl)
    {
        var userDb = await GetUserOrThrowAsync(email: model.Email);

        await SendEmailByEmailSubjectAsync(userDb, EmailTemplates.PasswordResetSubject, callBackUrl);       
    }
    
    public async ValueTask<UserActionToken> ValidateResetTokenAsync(string token, bool setUsed)
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

        return actionTokenDb;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest model)
    {
        var actionTokenDb = await ValidateResetTokenAsync(model.Token, true);        

        var userDb = await GetUserOrThrowAsync(userId: actionTokenDb.UserId);       

        var hashedPassword = _userRepository.HashPassword(userDb, model.NewPassword);

        userDb.PasswordHash = hashedPassword;

        var result = await _userRepository.UpdateAsync(userDb);

        EnsureSuccess(result);

        await SendEmailByEmailSubjectAsync(userDb, EmailTemplates.PasswordSuccessfullyResetSubject);
    }

    //helpers
    private async Task SendEmailByEmailSubjectAsync(User user, string emailSubject, string? callBackUrl = null)
    {
        string emailContent;
        string link;
        string expiresAfter;

        switch (emailSubject)
        {
            case EmailTemplates.EmailVerifySubject:
                (link, expiresAfter) = await GenerateTokenDetailsAsync(user, ActionTokenTypeEnum.EmailVerify, callBackUrl);
                emailContent = string.Format(EmailTemplates.EmailVerifyBody, user.Nickname, link, expiresAfter);
                break;

            case EmailTemplates.PasswordResetSubject:
                (link, expiresAfter) = await GenerateTokenDetailsAsync(user, ActionTokenTypeEnum.PasswordReset, callBackUrl);
                emailContent = string.Format(EmailTemplates.PasswordResetBody, user.Nickname, link, expiresAfter);               
                break;

            case EmailTemplates.PasswordSuccessfullyResetSubject:
                emailContent = string.Format(EmailTemplates.PasswordSuccessfullyResetBody, user.Nickname);
                break;

            default:
                throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed);
        }

        await _emailService.SendEmailAsync(user.Email, emailSubject, emailContent);
    }

    private async Task<(string Link, string Duration)> GenerateTokenDetailsAsync(User user, ActionTokenTypeEnum tokenType, string callBackUrl)
    {
        var actionToken = await UpsertActionTokenAsync(user.Id, tokenType);

        var link = $"{callBackUrl}?token={actionToken.Token}";

        var tokenSettings = GetTokenSettings(tokenType);

        var expiresAfter = $"{tokenSettings.Value} {TimeUnitEnumToFriendlyString(tokenSettings.Unit, tokenSettings.Value)}";

        return (link, expiresAfter);
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

    private async Task<UserActionToken> UpsertActionTokenAsync(string userId, ActionTokenTypeEnum tokenType)
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
                CreatedAt = created,
                ExpiresAt = expires
            };

            await _actionTokenRepository.CreateAsync(tokenDb);
        }
        else
        {
            tokenDb.Token = token;
            tokenDb.ExpiresAt = expires;
            tokenDb.CreatedAt = created;
            tokenDb.IsUsed = false;

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

    private async Task<User> GetUserOrThrowAsync(string? userId=null, string? email=null, bool throwIfUserExists = false)
    {
        User? userDb = null;

        if (userId != null)
        {
            userDb = await _userRepository.FindByIdAsync(userId);
        }
        else if (email != null)
        {
            email = email.Trim().ToLower();

            userDb = await _userRepository.FindByEmailAsync(email);
        }

        if (throwIfUserExists && userDb != null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.UserAlreadyExists);
        }

        if (!throwIfUserExists && userDb == null)
        {
            throw new KeyNotFoundException(ErrorMessages.UserNotFound);
        }

        return userDb;
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed);
        }
    }

    private async ValueTask<RefreshToken> ValidateRefreshToken(string token, string ipAddress)
    {
        var tokenDb = await _refreshTokenRepository.FindByTokenAndIpAsync(token, ipAddress);
        
        if (tokenDb == null || !tokenDb.IsActive)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.InvalidToken);
        }

        return tokenDb;
    }

    private async ValueTask<RefreshToken> GenerateUniqueRefreshToken()
    {
        RefreshToken newToken;

        do
        {
            newToken = _jwtService.GenerateRefreshTokenAsync();
        }
        while (await _refreshTokenRepository.FindByTokenAsync(newToken.Token) != null);

        return newToken;
    }

    private async Task UpdateTokenDbWithNewRefreshToken(RefreshToken tokenDb, RefreshToken newRefreshToken)
    {
        tokenDb.CreatedAt = newRefreshToken.CreatedAt;
        tokenDb.ExpiresAt = newRefreshToken.ExpiresAt;
        tokenDb.Token = newRefreshToken.Token;

        await _refreshTokenRepository.UpdateAsync(tokenDb);
    }

}
