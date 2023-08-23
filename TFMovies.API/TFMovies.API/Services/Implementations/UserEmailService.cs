using TFMovies.API.Common.Constants;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Services.Implementations;

public class UserEmailService : IUserEmailService
{
    private readonly IUserSecretTokenService _userSecretTokenService;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _emailTemplateService;

    public UserEmailService(
        IUserSecretTokenService userSecretTokenService,
        IEmailService emailService,
        IEmailTemplateService emailTemplateService)
    {
        _userSecretTokenService = userSecretTokenService;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
    }

    public async Task SendConfirmationLinkAsync(User user, string callBackUrl)
    {
        var secretToken = await _userSecretTokenService.GenerateAndStoreSecretTokenAsync(user.Id, SecretTokenTypeEnum.ConfirmEmail);
        var emailContent = _emailTemplateService.GetConfirmEmailContent(user, callBackUrl, secretToken.Token);
        await _emailService.SendEmailAsync(user.Email, EmailTemplates.ConfirmEmailSubject, emailContent);
    }

    public async Task SendPasswordRecoveryEmailAsync(User user, string callBackUrl)
    {
        var secretToken = await _userSecretTokenService.GenerateAndStoreSecretTokenAsync(user.Id, SecretTokenTypeEnum.ResetPassword);
        var emailContent = _emailTemplateService.GetPasswordRecoveryEmailContent(user, callBackUrl, secretToken.Token);
        await _emailService.SendEmailAsync(user.Email, EmailTemplates.PasswordRecoverySubject, emailContent);
    }
}
