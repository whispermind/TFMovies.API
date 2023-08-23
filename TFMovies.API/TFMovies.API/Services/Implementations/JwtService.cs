using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Services.Interfaces;
using TFMovies.API.Utils;

namespace TFMovies.API.Services.Implementations;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IJwtRefreshTokenRepository _jwtRefreshTokenRepository;

    public JwtService(
        IOptions<JwtSettings> jwtSettings, 
        IJwtRefreshTokenRepository jwtRefreshTokenRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _jwtRefreshTokenRepository = jwtRefreshTokenRepository;
    }  

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var expiryAt = TimeUtility.AddTime(DateTime.UtcNow, _jwtSettings.AccessTokenLifeTimeUnit, _jwtSettings.AccessTokenLifeTimeDuration);

        var token = CreateJwtToken(claims, _jwtSettings.ValidAudience, expiryAt);

        return token;
    }

    public async Task<string> GenerateRefreshTokenAsync(string userId)
    {
        var createdAt = DateTime.UtcNow;

        var expiryAt = TimeUtility.AddTime(createdAt, _jwtSettings.RefreshTokenLifeTimeUnit, _jwtSettings.RefreshTokenLifeTimeDuration);

        var refreshTokenId = Guid.NewGuid().ToString();

        var claims = new[] { new Claim("jti", refreshTokenId) };

        var token = CreateJwtToken(claims, _jwtSettings.ValidIssuer, expiryAt);

        await StoreTokenIntoDbAsync(userId, token, expiryAt, createdAt);

        return token;
    }

    public async Task ValidateAndMarkTokenAsync(string token, bool isRevoke = false)
    {
        var tokenDb = await _jwtRefreshTokenRepository.GetActiveTokenAsync(token);

        if (tokenDb == null)
        {
            throw new UnauthorizedAccessException();
        }        
        
        if (isRevoke)
        {
            tokenDb.IsRevoked = true; //for Logout
        }
        else
        {
            tokenDb.IsUsed = true;
        }        

        await _jwtRefreshTokenRepository.UpdateAsync(tokenDb);        
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidIssuer = _jwtSettings.ValidIssuer,
            ValidAudience = _jwtSettings.ValidAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SymmetricSecurityKey))
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            SecurityToken securityToken;

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch (Exception)
        {

            throw new InternalServerException(ErrorMessages.OperationFailed, new Exception(string.Format(ErrorMessages.OperationFailedDetails, "Expired Access Token validation.")));
        }
    }

    private string CreateJwtToken(IEnumerable<Claim> claims, string audience, DateTime expiryAt)
    {
        var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SymmetricSecurityKey)
        );

        var descriptor = new JwtSecurityToken(
            _jwtSettings.ValidIssuer,
            audience,
            expires: expiryAt,
            claims: claims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(descriptor);

        return token;
    }

    private async Task StoreTokenIntoDbAsync(string userId, string token, DateTime expiryAt, DateTime createdAt)
    {
        var userTokenDb = new JwtRefreshToken
        {
            UserId = userId,
            Token = token,
            CreatedAt = createdAt,
            ExpiryAt = expiryAt
        };

        var result = await _jwtRefreshTokenRepository.CreateAsync(userTokenDb);

        if (result == null)
        {
            throw new InternalServerException(ErrorMessages.OperationFailed, new Exception(string.Format(ErrorMessages.OperationFailedDetails, "Storing the Refresh token into the DB.")));
        }
    }    
}
