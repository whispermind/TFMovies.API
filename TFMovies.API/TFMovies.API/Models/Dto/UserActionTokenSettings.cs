namespace TFMovies.API.Models.Dto;

public class UserActionTokenSettings
{
    public TokenDurationSettings EmailVerifyDuration { get; set; }
    public TokenDurationSettings PasswordResetDuration { get; set; }
}
