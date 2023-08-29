namespace TFMovies.API.Common.Constants;

public static class ErrorMessages
{
    //Common
    public const string UnexpectedError = "An unexpected error has occurred. Please try again later.";
    public const string OperationFailed = "Something went wrong. Please try again later.";
    public const string InvalidRequestParams = "Invalid request parameters.";


    //User 
    //Model State -registration
    public const string UserAlreadyExists = "A user with the provided email already exists.";
    public const string InvalidToken = "Invalid token.";
    public const string IncorrectNickName = "{0} must start with an uppercase letter and contain only letters, minimum 2 symbols.";
    public const string IncorrectPasswordComplexity = "The Password must be at least 8 characters long and contain only Latin letters, at least one uppercase letter, one lowercase letter, one number, and one special character like -, _, +, =.";
    //other
    public const string IncorrectPassword = "Password is incorrect.";
    public const string UserNotFound = "User with the provided email not found.";
    public const string UnconfirmedEmail = "Email address has not been confirmed yet. Please check your inbox for the confirmation link.";


    ////For Inner exception
    //public const string UserWithoutRoles = "User has no roles.";
    //public const string OperationFailedDetails = "An error occurred during {0}. "; //0 - some operation/process
    //public const string RefreshTokenUserNotFound = "No user associated with the provided refresh token.";
    //public const string SendgridProblem = "Sendgrid problem: {0} - {1}."; //0-StatusCode, 1 - SendgridException
}