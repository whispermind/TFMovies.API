namespace TFMovies.API.Common.Constants;

public static class EmailTemplates
{
    public const string EmailVerifySubject = "Verify Your Email";

    public const string PasswordResetSubject = "Password Recovery Request";

    public const string PasswordSuccessfullyResetSubject = "Password Successfully Changed";

    public const string RoleChangedSubject = "Your Role Has Been Updated";

    public const string RoleChangeRequestSubject = "Role Change Request Received";


    public const string EmailVerifyBody = @"
        <p>Welcome!</p>        
        <p><strong>{0}</strong>,</p>
        <p>Please confirm your email by clicking <a href='{1}'>here</a>.</p>
        <p><strong>The link is valid for {2}.</strong></p>
        <br>
        <p><i>If you received this e-mail by mistake, please ignore it.</i></p>
        <br>
        <p><i><strong>Regards,<br>TFMovies Team</strong></i></p>";

    public const string PasswordResetBody = @"                
        <p>Hello <strong>{0}</strong>,</p>
        <p>We received a request to reset your password.</p>
        <p><i>If you didn't make this request, you can safely ignore this email.</i></p>
        <p>Otherwise, please click on the link below to proceed with resetting your password:</p>
        <p><a href='{1}'>Reset My Password</a></p>
        <p><strong>Please note that this reset token is valid for {2} only.</strong></p>
        <p><strong>After this period, you'll need to request a new one if you still wish to reset your password.</strong></p>         
        <br>
        <p><i><strong>Regards,<br>TFMovies Team</strong></i></p>";

    public const string PasswordSuccessfullyResetBody = @"
        <p>Hello <strong>{0}</strong>,</p>
        <p>Your password has been successfully reset.</p>
        <br>
        <p><strong><i>If you didn't make this change or if you believe an unauthorized person has accessed your account, please reset your password immediately and contact our support team.</i></strong></p>
        <p>To keep your account safe, always ensure that you're using a strong and unique password, and never share it with anyone.</p>
        <br>
        <p><i>If you face any issues or have any questions, feel free to reach out to our support team. We're here to help!</i></p>        
        <p><i><strong>Regards,<br>TFMovies Team</strong></i></p>";

    public const string RoleChangedBody = @"
        <p>Hello <strong>{0}</strong>,</p>
        <p>Congratulations! Your role on <strong>TFMovies</strong> has been successfully updated to <strong>{1}</strong>.</p>
        <br>
        <p><strong>Important:</strong> If you are currently logged in to TFMovies, please <strong>log out and log back in</strong> to ensure your new role settings are applied correctly.</p>
        <br>
        <p>With this new role, you might have access to additional features or functionalities on our platform.</p>
        <p><i>We encourage you to explore them and take full advantage of your new privileges.</i></p>
        <p>If you did not request this change or if you believe this is an error, please <strong>contact our support team</strong> immediately for assistance.</p>
        <br>
        <p>We're constantly working to enhance our platform and provide you with the best experience possible.</p>
        <p>If you have any feedback or questions regarding your new role or any other aspect of TFMovies, please don't hesitate to reach out.</p>
        <br>
        <p><i><strong>Regards,<br>TFMovies Team</strong></i></p>";

    public const string RoleChangeRequestBody = @"
        <p>Hello <strong>{0}</strong>,</p>
        <p>We have received <strong>your request to change your role on TFMovies</strong>.</p>
        <p><i>Please note that your request is currently under consideration.</i> We appreciate your patience during this time.</p>
        <p>Once a decision has been made regarding your request, we will notify you with further details.</p>
        <br>
        <p>If you believe this request was made in error or did not request a role change, please <strong>contact our support team</strong> immediately.</p>
        <p><i>Thank you for using TFMovies. We value your participation and strive to provide the best experience possible.</i></p>
        <br>
        <p><i><strong>Regards,<br>TFMovies Team</strong></i></p>";
}
