using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;

namespace TFMovies.API.Mappers;

public static class RefreshTokenMapper
{
    public static RefreshToken ToRefreshToken(string token, DateTime created, DateTime expires)
    {
        return new RefreshToken
        {
            Token = token,
            CreatedAt = created,
            ExpiresAt = expires
        };
    }
}
