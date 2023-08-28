using TFMovies.API.Data.Entities;

namespace TFMovies.API.Services.Interfaces;

public interface IUserEmailService
{
    public Task SendConfirmationLinkAsync(User user, string callBackUrl);
    public Task SendPasswordRecoveryEmailAsync(User user, string callBackUrl);
}
