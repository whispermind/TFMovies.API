﻿using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IUserService
{
    public ValueTask<JwtTokensResponse> LoginAsync(LoginRequest model, string callBackUrl, string ipAdress);
    public ValueTask<JwtTokensResponse> RefreshJwtTokens(RefreshTokenRequest model, string ipAdress);    
    public Task RegisterAsync(RegisterRequest model, string callBackUrl);
    public Task VerifyEmailAsync(VerifyEmailRequest model);
    public Task SendActivationEmailAsync(ActivateEmailRequest model, string callBackUrl);
    public Task ForgotPasswordAsync(ForgotPasswordRequest model, string callBackUrl);
    public Task ValidateResetTokenAsync(string token, bool setUsed);
    public Task ResetPasswordAsync(ResetPasswordRequest model);
}