using Microsoft.Extensions.Options;
using TFMovies.API.Common.Constants;
using TFMovies.API.Common.Enum;
using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Services.Implementations;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly ConfirmEmailTokenSettings _confirmEmailTokenSettings;
    private readonly ResetPasswordTokenSettings _passwordResetTokenSettings;

    public EmailTemplateService(
        IOptions<ResetPasswordTokenSettings> passwordResetTokenSettings, 
        IOptions<ConfirmEmailTokenSettings> confirmEmailTokenSettings)
    {
        _passwordResetTokenSettings = passwordResetTokenSettings.Value;
        _confirmEmailTokenSettings = confirmEmailTokenSettings.Value;
    }

    public string GetConfirmEmailContent(User user, string callBackUrl, string token)
    {
        var link = GenerateLinkWithToken(callBackUrl, token, user.Email);

        var tokenLifeTime = GetTokenLifetimeString(_confirmEmailTokenSettings);

        var emailContent = string.Format(EmailTemplates.ConfirmEmailBody, user.Nickname, link, tokenLifeTime);

        return emailContent;
    }

    public string GetPasswordRecoveryEmailContent(User user, string callBackUrl, string token)
    {
        var link = GenerateLinkWithToken(callBackUrl, token, user.Email);

        var tokenLifeTime = GetTokenLifetimeString(_passwordResetTokenSettings);

        var emailContent = string.Format(EmailTemplates.PasswordRecoveryBody, user.Nickname, link, tokenLifeTime);

        return emailContent;
    }

    private static string GenerateLinkWithToken(string baseUrl, string token, string email)
    {
        return $"{baseUrl}?token={token}?email={email}";
    }
    private string GetTokenLifetimeString(ISecretTokenSettings settings)
    {
        return $"{settings.LifeTimeDuration} {TimeUnitEnumToFriendlyString(settings.LifeTimeUnit, settings.LifeTimeDuration)}";
    }

    private static string TimeUnitEnumToFriendlyString(TimeUnitEnum unit, int duration)
    {
        string baseStr = unit.ToString();

        if (duration == 1)
        {
            return baseStr.TrimEnd('s');
        }

        return baseStr;
    }
}
