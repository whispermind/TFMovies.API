using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using TFMovies.API.Common.Constants;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;
using TFMovies.API.Exceptions;
using TFMovies.API.Integrations;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Repositories.Implementations;
using TFMovies.API.Repositories.Interfaces;
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
    private readonly IPostLikeRepository _postLikeRepository;

    public UserService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IUserActionTokenRepository actionTokenRepository,
        IOptions<UserActionTokenSettings> actionTokenSettings,
        IRefreshTokenRepository refreshTokenRepository,
        IEmailService emailService,
        IPostLikeRepository postLikeRepository)

    {
        _userRepository = userRepository;
        _actionTokenRepository = actionTokenRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _actionTokenSettings = actionTokenSettings.Value;
        _emailService = emailService;
        _postLikeRepository = postLikeRepository;
    }
    public async Task<LoginResponse> LoginAsync(LoginRequest model, string callBackUrl)
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

        var roleDetails = await _userRepository.GetUserRoleDetailsAsync(userDb);

        if (roleDetails == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.UserMissingRoleError);
        }

        var accessToken = _jwtService.GenerateAccessToken(userDb, roleDetails.Name);

        var refreshToken = await GenerateUniqueRefreshToken();

        refreshToken.UserId = userDb.Id;        

        await _refreshTokenRepository.CreateAsync(refreshToken);

        return new LoginResponse
        {
            CurrentUser = new UserShortInfoDto
            {
                Id = userDb.Id,
                Nickname = userDb.Nickname,
                Email = userDb.Email,
                Role = roleDetails
            },
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };
    }

    public async Task LogoutAsync(LogoutRequest model)
    {
        var tokenDb = await _refreshTokenRepository.FindByTokenAsync(model.RefreshToken);

        if (tokenDb != null)
        {
            tokenDb.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.SaveChangesAsync();
        }
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

        var newAccessToken = _jwtService.GenerateAccessToken(userDb, userRole);

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

        var email = model.Email.ToLower();

        var newUser = new User
        {
            UserName = email,
            Email = email,
            Nickname = model.Nickname
        };

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
        var userDb = await GetUserOrThrowAsync(model.Email);

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

    public async Task ChangeRoleAsync(string newRole, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await UserUtils.GetUserByIdFromClaimAsync(_userRepository, currentUserPrincipal);

        if (currentUser == null)
        {
            throw new ServiceException(HttpStatusCode.Unauthorized, ErrorMessages.UserNotFound);
        }

        if (!IsValidRole(newRole))
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.InvalidRole);
        }

        var currentRoles = await _userRepository.GetRolesAsync(currentUser);

        await _userRepository.RemoveFromRolesAsync(currentUser, currentRoles);

        var result = await _userRepository.AddToRoleAsync(currentUser, newRole);

        if (!result.Succeeded)
        {
            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.UpdateRoleFailed);
        }
    }

    public async Task<IEnumerable<UserShortInfoDto>> GetAuthorsAsync(PagingSortParams model)
    {           
        var topUsersByPostLikeCounts = await _postLikeRepository.GetUserIdsByPostLikeCountsAsync(model.Limit, model.Order);

        var userIds = topUsersByPostLikeCounts.Select(a => a.AuthorId).ToList();

        var users = await _userRepository.GetUsersByIdsAsync(userIds);
        
        var result = users?.Select(a => new UserShortInfoDto
        {
            Id = a.Id,
            Nickname = a.Nickname,
            Email = a.Email
        }) ?? Enumerable.Empty<UserShortInfoDto>();

        return result;
    }

    public async Task<UsersPaginatedResponse> GetAllPagingAsync(PagingSortParams pagingSortModel, UsersFilterParams filterModel, UsersQueryParams queryModel, ClaimsPrincipal currentUserPrincipal)
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

        var queryDto = new UsersQueryDto
        {
            Query = termsQuery
        };

        if (string.IsNullOrEmpty(pagingSortModel.Order))
        {
            pagingSortModel.Order = OrderOptions.Asc;
        }

        var pagedUsers = await _userRepository.GetAllPagingAsync(pagingSortModel, filterModel, queryDto);

        var response = MapToUsersPaginatedResponse(pagedUsers);

        return response;
    }

    //helpers
    private UsersPaginatedResponse MapToUsersPaginatedResponse(PagedResult<UserRoleDto> pagedUsers)
    {
        var data = pagedUsers.Data.Select(ur => new UserShortInfoDto
        {
            Id = ur.User.Id,
            Nickname = ur.User.Nickname,
            Email = ur.User.Email,
            Role = ur.Role

        }).ToList();

        return new UsersPaginatedResponse
        {
            Page = pagedUsers.Page,
            Limit = pagedUsers.Limit,
            TotalPages = pagedUsers.TotalPages,
            TotalRecords = pagedUsers.TotalRecords,
            Data = data
        };
    }
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

        await _emailService.SendEmailAsync(user.Email, emailSubject, emailContent);
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
        tokenDb.CreatedAt = newRefreshToken.CreatedAt;
        tokenDb.ExpiresAt = newRefreshToken.ExpiresAt;
        tokenDb.Token = newRefreshToken.Token;

        await _refreshTokenRepository.UpdateAsync(tokenDb);
    }

    private bool IsValidRole(string role)
    {
        var validRoles = new List<string> { RoleNames.Admin, RoleNames.User, RoleNames.Author };

        return validRoles.Contains(role);
    }
}
