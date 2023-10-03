using Newtonsoft.Json.Linq;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Mappers;

public static class UserMapper
{
    public static UserShortInfoDto ToUserShortInfoDto(User user, RoleDto? roleDetails)
    {
        var result = new UserShortInfoDto
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Email = user.Email,
            Role = roleDetails
        };

        return result;
    }

    public static LoginResponse ToLoginResponse(UserShortInfoDto userInfo, string accessTokenVal, string refreshTokenVal)
    {
        var result = new LoginResponse
        {
            CurrentUser = userInfo,
            AccessToken = accessTokenVal,
            RefreshToken = refreshTokenVal
        };

        return result;
    }

    public static JwtTokensResponse ToJwtTokensResponse(string accessTokenVal, string refreshTokenVal)
    {
        var result = new JwtTokensResponse
        {
            AccessToken = accessTokenVal,
            RefreshToken = refreshTokenVal
        };

        return result;
    }

    public static User ToCreateEntity(string email, string nickname)
    {       
        var result = new User
        {
            UserName = email,
            Email = email,
            Nickname = nickname
        };

        return result;
    }

    public static UsersQueryDto ToQueryDto(IEnumerable<string> termsQuery)
    {
        var result = new UsersQueryDto
        {
            Query = termsQuery
        };

        return result;
    }

    public static UserActionToken ToUserActionToken(string userId, string tokenValue, ActionTokenTypeEnum tokenType, DateTime created, DateTime expires)
    {
        var result = new UserActionToken
        {
            UserId = userId,
            Token = tokenValue,
            TokenType = tokenType,
            CreatedAt = created,
            ExpiresAt = expires
        };

        return result;
    }

    public static RefreshToken ToUpdateRefreshToken (RefreshToken targetToken, RefreshToken sourceToken)
    {
        targetToken.CreatedAt = sourceToken.CreatedAt;
        targetToken.ExpiresAt = sourceToken.ExpiresAt;
        targetToken.Token = sourceToken.Token;

        return targetToken;
    }
}
