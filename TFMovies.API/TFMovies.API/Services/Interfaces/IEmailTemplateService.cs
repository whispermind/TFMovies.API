using TFMovies.API.Data.Entities;

namespace TFMovies.API.Services.Interfaces;

public interface IEmailTemplateService
{
    public string GetConfirmEmailContent(User user, string callBackUrl, string token);
    public string GetPasswordRecoveryEmailContent(User user, string callBackUrl, string token);
}
