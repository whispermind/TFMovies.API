﻿namespace TFMovies.API.Common.Constants;

public static class EmailTemplates
{
    public const string EmailVerifySubject = "Verify Your Email";

    public const string PasswordResetSubject = "Password Recovery Request";

    public const string PasswordSuccessfullyResetSubject = "Password Successfully Changed";
    

    public const string EmailVerifyBody = @"
        <p>Welcome!</p>        
        <p>{0},</p>
        <p>Please confirm your email by clicking <a href='{1}'>here</a>.</p>
        <p><strong>The link is valid for {2}.</strong></p>
        <br>
        <p><i>If you received this e-mail by mistake, please ignore it.</i></p>
        <br>
        <p><i><strong>Regards, TFMovies</strong></i></p>";

    public const string PasswordResetBody = @"                
        <p>Hello {0},</p>
        <p>We received a request to reset your password.</p>
        <p><i>If you didn't make this request, you can safely ignore this email.</i></p>
        <p>Otherwise, please click on the link below to proceed with resetting your password:</p>
        <p><a href='{1}'>Reset My Password</a></p>
        <p><strong>Please note that this reset token is valid for {2} only.</strong></p>
        <p><strong>After this period, you'll need to request a new one if you still wish to reset your password.</strong></p>         
        <br>
        <p><i><strong>Regards, TFMovies</strong></i></p>";

    public const string PasswordSuccessfullyResetBody = @"
        <p>Hello {0},</p>
        <p>Your password has been successfully reset.</p>
        <br>
        <p><strong><i>If you didn't make this change or if you believe an unauthorized person has accessed your account, please reset your password immediately and contact our support team.</i></strong></p>
        <p>To keep your account safe, always ensure that you're using a strong and unique password, and never share it with anyone.</p>
        <br>
        <p><i>If you face any issues or have any questions, feel free to reach out to our support team. We're here to help!</i></p>        
        <p><i><strong>Regards, TFMovies</strong></i></p>";    
}
