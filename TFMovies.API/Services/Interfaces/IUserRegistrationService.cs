using TFMovies.API.Models.Requests;

namespace TFMovies.API.Services.Interfaces;

public interface IUserRegistrationService
{
    public Task RegisterAsync(UserRegisterRequest model, string callBackUrl);
    public Task ConfirmEmailByTokenAsync(string token);
    public Task SendConfirmationLinkToEmailAsync(string email, string callBackUrl);
}
