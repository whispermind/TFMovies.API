namespace TFMovies.API.Common.Constants;

public static class EmailTemplates
{
    public const string ConfirmationEmailTemplate = @"
        <p>Welcome!</p>        
        <p>{0},</p>
        <p>Please confirm your email by clicking <a href='{1}'>here</a>.</p>
        <p><strong>The link is valid for {2}.</strong></p>
        <br>
        <p><i>If you received this e-mail by mistake, please ignore it.</i></p>
        <br>
        <p>Regards, TFMovies</p>";

    public const string PasswordRecoveryEmailTemplate = @"
        <p>Password Recovery Request</p>        
        <p>Hello {0},</p>
        <p>We received a request to reset your password. If you didn't make this request, you can safely ignore this email. Otherwise, please click on the link below to proceed with resetting your password:</p>
        <p><a href='{1}'>Reset My Password</a></p>
        <p><strong>Please note that this reset token is valid for {2} only. After this period, you'll need to request a new one if you still wish to reset your password.</strong></p>
        <br>
        <p><i>If you received this e-mail by mistake, please ignore it.</i></p>
        <br>
        <p>Regards, TFMovies</p>";
}
