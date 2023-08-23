using System.Security.Claims;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;
using TFMovies.API.Utils;

namespace TFMovies.API.Services.Implementations;

public class UserAuthenticationService : IUserAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUserSecretTokenService _userSecretTokenService;
    private readonly IUserEmailService _userEmailService;

    public UserAuthenticationService(
        IUserRepository userRepository, 
        IJwtService jwtService,
        IUserSecretTokenService userSecretTokenService,
        IUserEmailService userEmailService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _userSecretTokenService = userSecretTokenService;
        _userEmailService = userEmailService;       
    }
    public async ValueTask<JwtTokensResponse> LoginAsync(UserLoginRequest model, string callBackUrl)
    {
        var email = model.Email.ToLower();

        var userDb = await _userRepository.FindByEmailAsync(email);

        if (userDb == null)
        {
            throw new KeyNotFoundException(ErrorMessages.UserNotFound);
        }

        var isPasswordValid = await _userRepository.CheckPasswordAsync(userDb, model.Password);

        if (!isPasswordValid)
        {
            throw new BadRequestException(ErrorMessages.IncorrectPassword);
        }

        if (!userDb.EmailConfirmed)
        {
            await _userEmailService.SendConfirmationLinkAsync(userDb, callBackUrl);

            throw new BadRequestException(ErrorMessages.UnconfirmedEmail);
        }

        var tokenClaims = await GenerateClaimsForUserAsync(userDb);

        var accessToken = _jwtService.GenerateAccessToken(tokenClaims);

        var refeshToken = await _jwtService.GenerateRefreshTokenAsync(userDb.Id);

        var result = new JwtTokensResponse
        {
            AccessToken = accessToken,
            RefreshToken = refeshToken
        };

        return result;
    }

    public async Task<JwtTokensResponse> RefreshJwtTokens(RefreshTokenRequest model)
    {
        var principal = _jwtService.GetPrincipalFromToken(model.AccessToken);

        if (principal == null)
        {
            throw new UnauthorizedAccessException();
        }

        var userId = principal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        await _jwtService.ValidateAndMarkTokenAsync(model.RefreshToken);

        var newAccessToken = _jwtService.GenerateAccessToken(principal.Claims);

        var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync(userId);

        var result = new JwtTokensResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };

        return result;
    }

    private async Task<IEnumerable<Claim>> GenerateClaimsForUserAsync(User userDb)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userDb.Id),
                new Claim(ClaimTypes.Email, userDb.Email)
        };

        var userRoles = await _userRepository.GetRolesAsync(userDb);

        if (!userRoles.Any())
        {
            throw new InternalServerException(ErrorMessages.OperationFailed, new Exception($"{string.Format(ErrorMessages.OperationFailedDetails, "AccessToken generation")}: {ErrorMessages.UserWithoutRoles}"));
        }

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }    
}
