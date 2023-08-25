using Microsoft.AspNetCore.Identity;

namespace TFMovies.API.Data.Entities;

public class User : IdentityUser
{
    public string? Nickname { get; set; }
    public ICollection<UserSecretToken>? UserSecretTokens { get; set; }
    public ICollection<JwtRefreshToken>? JwtRefreshTokens { get; set; }
}
