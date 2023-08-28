using System.Security.Claims;
using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IJwtService
{
    public string GenerateAccessToken(IEnumerable<Claim> claims);
    public Task<string> GenerateRefreshTokenAsync(string userId);    
    public ClaimsPrincipal? GetPrincipalFromToken(string token);
    public Task ValidateAndMarkTokenAsync(string token, bool isRevoke = false);
}
