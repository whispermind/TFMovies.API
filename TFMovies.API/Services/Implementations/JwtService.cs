using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;
using TFMovies.API.Services.Interfaces;
using TFMovies.API.Utils;

namespace TFMovies.API.Services.Implementations;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;   

    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;        
    }  

    public string GenerateAccessToken(User user, string userRole)
    {
        var expiryAt = TimeUtility.AddTime(DateTime.UtcNow, _jwtSettings.AccessTokenLifeTimeUnit, _jwtSettings.AccessTokenLifeTimeDuration);

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id),
            new Claim("email", user.Email)
        };

       
        claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, userRole));
        

        var token = CreateJwtToken(claims, _jwtSettings.ValidAudience, expiryAt);

        return token;
    }

    public RefreshToken GenerateRefreshTokenAsync()
    {
        var created = DateTime.UtcNow;

        var expires = TimeUtility.AddTime(created, _jwtSettings.RefreshTokenLifeTimeUnit, _jwtSettings.RefreshTokenLifeTimeDuration);

        var refreshTokenId = Guid.NewGuid().ToString();

        var claims = new[] { new Claim("jti", refreshTokenId) };

        var token = CreateJwtToken(claims, _jwtSettings.ValidIssuer, expires);

        var refreshToken = new RefreshToken
        {            
            Token = token,
            CreatedAt = created,            
            ExpiresAt = expires
        };                

        return refreshToken;
    }   

    private string CreateJwtToken(IEnumerable<Claim> claims, string audience, DateTime expires)
    {
        var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SymmetricSecurityKey));

        var descriptor = new JwtSecurityToken(
            _jwtSettings.ValidIssuer,
            audience,
            expires: expires,
            claims: claims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(descriptor);

        return token;
    }      
}
