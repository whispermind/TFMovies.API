using TFMovies.API.Common.Enum;

namespace TFMovies.API.Models.Dto;

public class JwtSettings
{
    public string? ValidIssuer { get; set; }
    public string? ValidAudience { get; set; }
    public string? SymmetricSecurityKey { get; set; }
    public int AccessTokenLifeTimeDuration { get; set; }
    public TimeUnitEnum AccessTokenLifeTimeUnit { get; set; }
    public int RefreshTokenLifeTimeDuration { get; set; }
    public TimeUnitEnum RefreshTokenLifeTimeUnit { get; set; }
}

