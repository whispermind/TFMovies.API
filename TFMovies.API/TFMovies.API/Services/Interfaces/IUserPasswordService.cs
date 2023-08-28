using TFMovies.API.Models.Requests;

namespace TFMovies.API.Services.Interfaces;

public interface IUserPasswordService
{
    public Task SendResetPasswordLinkToEmailAsync(string email, string callBackUrl);
    public Task VerifyResetTokenAsync(string token, bool setIsUsed);
    public Task RecoveryPasswordAsync(RecoveryPasswordRequest model);
}
