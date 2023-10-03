using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using TFMovies.API.Common.Constants;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;
using TFMovies.API.Exceptions;
using TFMovies.API.Integrations;
using TFMovies.API.Mappers;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Repositories.Interfaces;
using TFMovies.API.Services.Interfaces;
using TFMovies.API.Utils;

namespace TFMovies.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserActionTokenRepository _actionTokenRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly UserActionTokenSettings _actionTokenSettings;
    private readonly IPostLikeRepository _postLikeRepository;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;


    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IJwtService jwtService,
        IUserActionTokenRepository actionTokenRepository,
        IOptions<UserActionTokenSettings> actionTokenSettings,
        IRefreshTokenRepository refreshTokenRepository,
        IEmailService emailService,
        IPostLikeRepository postLikeRepository,
        IBackgroundTaskQueue backgroundTaskQueue)

    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _actionTokenRepository = actionTokenRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _actionTokenSettings = actionTokenSettings.Value;
        _emailService = emailService;
        _postLikeRepository = postLikeRepository;
        _backgroundTaskQueue = backgroundTaskQueue;
    }
    public async Task<LoginResponse> LoginAsync(LoginRequest model, string callBackUrl)
    {
        var email = model.Email.ToLower();

        var userDb = await _userRepository.FindByEmailAsync(email);

        var isPasswordValid = await _userRepository.CheckPasswordAsync(userDb, model.Password);

        if (userDb == null || !isPasswordValid)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.LoginFailed);
        }

        if (!userDb.EmailConfirmed)
        {
            await SendEmailByEmailSubjectAsync(userDb, EmailTemplates.EmailVerifySubject, callBackUrl);

            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.UnconfirmedEmail);
        }

        var roleDetails = await _userRepository.GetUserRoleDetailsAsync(userDb);

        if (roleDetails == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.UserMissingRoleError);
        }

        var accessTokenVal = _jwtService.GenerateAccessToken(userDb, roleDetails.Name);

        var refreshToken = await GenerateUniqueRefreshToken();

        refreshToken.UserId = userDb.Id;

        await _refreshTokenRepository.CreateAsync(refreshToken);

        var userShortInfo = UserMapper.ToUserShortInfoDto(userDb, roleDetails);

        var response = UserMapper.ToLoginResponse(userShortInfo, accessTokenVal, refreshToken.Token);

        return response;        
    }

    public async Task LogoutAsync(LogoutRequest model)
    {
        var tokenDb = await _refreshTokenRepository.FindByTokenAsync(model.RefreshToken);

        if (tokenDb == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.InvalidToken);
        }

        tokenDb.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.SaveChangesAsync();
    }

    public async Task<JwtTokensResponse> RefreshJwtTokens(RefreshTokenRequest model)
    {
        var tokenDb = await ValidateRefreshToken(model.RefreshToken);

        var userDb = await GetUserOrThrowAsync(userId: tokenDb.UserId);

        var userRole = (await _userRepository.GetRolesAsync(userDb)).FirstOrDefault();

        if (userRole == null)
        {
            userRole = "Undefined";
        }

        var newAccessTokenVal = _jwtService.GenerateAccessToken(userDb, userRole);

        var newRefreshToken = await GenerateUniqueRefreshToken();

        await UpdateTokenDbWithNewRefreshToken(tokenDb, newRefreshToken);

        return UserMapper.ToJwtTokensResponse(newAccessTokenVal, newRefreshToken.Token);
    }

    public async Task RegisterAsync(RegisterRequest model, string callBackUrl)
    {
        await GetUserOrThrowAsync(email: model.Email, throwIfUserExists: true);

        var email = model.Email.ToLower();

        var newUser = UserMapper.ToCreateEntity(email, model.Nickname);

        var result = await _userRepository.CreateAsync(newUser, model.Password);

        EnsureSuccess(result);

        result = await _userRepository.AddToRoleAsync(newUser, RoleNames.User);

        if (!result.Succeeded)
        {
            await _userRepository.DeleteAsync(newUser);

            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed);
        }

        await SendEmailByEmailSubjectAsync(newUser, EmailTemplates.EmailVerifySubject, callBackUrl);
    }

    public async Task VerifyEmailAsync(EmailVerifyRequest model)
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

    public async Task SendActivationEmailAsync(EmailActivateRequest model, string callBackUrl)
    {
        var userDb = await GetUserOrThrowAsync(email: model.Email);

        await SendEmailByEmailSubjectAsync(userDb, EmailTemplates.EmailVerifySubject, callBackUrl);
    }

    public async Task ForgotPasswordAsync(PasswordForgotRequest model, string callBackUrl)
    {
        var userDb = await GetUserOrThrowAsync(email: model.Email);

        await SendEmailByEmailSubjectAsync(userDb, EmailTemplates.PasswordResetSubject, callBackUrl);
    }

    public async Task<UserActionToken> ValidateResetTokenAsync(string token, bool setUsed)
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

    public async Task ResetPasswordAsync(PasswordResetRequest model)
    {
        var actionTokenDb = await ValidateResetTokenAsync(model.Token, true);

        var userDb = await GetUserOrThrowAsync(userId: actionTokenDb.UserId);

        var hashedPassword = _userRepository.HashPassword(userDb, model.NewPassword);

        userDb.PasswordHash = hashedPassword;

        var result = await _userRepository.UpdateAsync(userDb);

        EnsureSuccess(result);

       await SendEmailByEmailSubjectAsync(userDb, EmailTemplates.PasswordSuccessfullyResetSubject, null);
    }

    public async Task ChangeRoleAsync(string id, ChangeRoleRequest model)
    {
        var userDb = await GetUserOrThrowAsync(userId: id);

        var newRole = await _roleRepository.FindByIdAsync(model.NewRoleId);

        if (newRole == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.InvalidRole);
        }        

        var currentRoles = await _userRepository.GetRolesAsync(userDb);

        await _userRepository.RemoveFromRolesAsync(userDb, currentRoles);

        var result = await _userRepository.AddToRoleAsync(userDb, newRole.Name);

        if (!result.Succeeded)
        {
            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.UpdateRoleFailed);
        }
    }

    public async Task<IEnumerable<UserShortInfoDto>> GetAuthorsAsync(PagingSortParams model)
    {           
        var topUsersByPostLikeCounts = await _postLikeRepository.GetUserIdsByPostLikeCountsAsync(model.Limit, model.Order);

        var userIdsOrdered = topUsersByPostLikeCounts.Select(a => a.AuthorId).ToList();

        var usersUnordered = await _userRepository.GetUsersByIdsAsync(userIdsOrdered);

        var usersOrdered = userIdsOrdered
           .Join(usersUnordered,
                 id => id,
                 user => user.Id,
                 (id, user) => user)
           .ToList();

        var result = usersOrdered?
            .Select(user => UserMapper.ToUserShortInfoDto(user, null)) ?? Enumerable.Empty<UserShortInfoDto>();

        return result;
    }

    public async Task<PagedResult<UserShortInfoDto>> GetAllPagingAsync(PagingSortParams pagingSortModel, UsersFilterParams filterModel, UsersQueryParams queryModel, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await UserUtils.GetUserByIdFromClaimAsync(_userRepository, currentUserPrincipal);

        UserUtils.CheckCurrentUserFoundOrThrow(currentUser);

        var termsQuery = Enumerable.Empty<string>();

        if (!string.IsNullOrEmpty(queryModel.Users))
        {
            termsQuery = StringParsingHelper.ParseDelimitedValues(queryModel.Users);
        }
        else if (string.IsNullOrEmpty(queryModel.Users) && !await _userRepository.IsInRoleAsync(currentUser, RoleNames.Admin))
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.SearchFailedNoValuesProvided);
        }

        var queryDto = UserMapper.ToQueryDto(termsQuery);       

        if (string.IsNullOrEmpty(pagingSortModel.Order))
        {
            pagingSortModel.Order = OrderOptions.Asc;
        }

        var pagedUsers = await _userRepository.GetAllPagingAsync(pagingSortModel, filterModel, queryDto);

        var response = GenericMapper.ToPaginatedResponse(pagedUsers, ur => UserMapper.ToUserShortInfoDto(ur.User, ur.Role));

        return response;
    }

    public async Task SoftDeleteAsync(string id)
    {
        var user = await _userRepository.FindByIdAsync(id);

        if (user != null)
        {
            await _userRepository.SoftDeleteAsync(user);
        }        
    }


    //helpers   
    private async Task SendEmailByEmailSubjectAsync(User user, string emailSubject, string? callBackUrl)
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

        _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
        {
            await _emailService.SendEmailAsync(user.Email, emailSubject, emailContent);
        });
    }

    private async Task<(string Link, string Duration)> GenerateTokenDetailsAsync(User user, ActionTokenTypeEnum tokenType, string? callBackUrl)
    {
        var actionToken = await UpsertActionTokenAsync(user.Id, tokenType);

        var link = $"{callBackUrl}/{actionToken.Token}";

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
        var tokenValue = await GenerateActionTokenAsync();

        var created = DateTime.UtcNow;

        var tokenSettings = GetTokenSettings(tokenType);

        var expires = TimeUtility.AddTime(created, tokenSettings.Unit, tokenSettings.Value);

        var newToken = UserMapper.ToUserActionToken(userId, tokenValue, tokenType, created, expires);        

        await _actionTokenRepository.UpsertAsync(newToken);

        return newToken;
    }
    private async Task<string> GenerateActionTokenAsync()
    {
        const int maxAttempts = 5;

        for (int i = 0; i < maxAttempts; i++)
        {
            var tokenValue = Guid.NewGuid().ToString();
            var tokenIsUnique = !await _actionTokenRepository.IsTokenValueExistsAsync(tokenValue);
            if (tokenIsUnique)
            {
                return tokenValue;
            }
        }

        throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.GenerateUniqueTokenFailed);       
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

    private async Task<User?> GetUserOrThrowAsync(string? userId = null, string? email = null, bool throwIfUserExists = false)
    {
        User? userDb = null;

        if (userId != null)
        {
            userDb = await _userRepository.FindByIdAsync(userId);
        }
        else if (email != null)
        {
            email = email.ToLower();

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

    private async ValueTask<RefreshToken> ValidateRefreshToken(string token)
    {
        var tokenDb = await _refreshTokenRepository.FindByTokenAsync(token);

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
        UserMapper.ToUpdateRefreshToken(tokenDb, newRefreshToken);

        await _refreshTokenRepository.UpdateAsync(tokenDb);
    }
}
