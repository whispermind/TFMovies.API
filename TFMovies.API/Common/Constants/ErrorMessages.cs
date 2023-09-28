namespace TFMovies.API.Common.Constants;

public static class ErrorMessages
{
    //Common
    public const string UnexpectedError = "An unexpected error has occurred. Please try again later.";
    public const string OperationFailed = "Something went wrong. Please try again later.";

    //User 
    //Model State -registration
    public const string UserAlreadyExists = "A user with the provided email already exists.";
    public const string EmailInvalidFormat = "The email address is not in a valid format. Please enter a valid email address.";
    public const string InvalidToken = "Invalid token.";
    public const string IncorrectNickName = "{0} must start with an uppercase letter and contain only Latin letters, minimum 2 symbols.";
    public const string IncorrectPasswordComplexity = "The Password must be at least 8 characters long and contain only Latin letters, at least one uppercase letter, one lowercase letter, one number, and one special character like -, _, +, =.";
    //other
    public const string IncorrectPassword = "Password is incorrect.";
    public const string UserNotFound = "User with the provided email not found.";
    public const string UnconfirmedEmail = "Email address has not been confirmed yet. Please check your inbox for the confirmation link.";
    public const string InvalidRole = "Invalid Role specified.";
    public const string UpdateRoleFailed = "Error updating user role";
    public const string SearchColumnsNotDefined = "Search Columns doesn't defined in derived repository.";
    public const string UserMissingRoleError = "The user does not have an assigned role.";
    public const string SearchFailedNoValuesProvided = "No search values provided. Please enter a value to proceed with the search.";

    // Files exception
    public const string UploadedFileInvalid = "No file uploaded or the file name is invalid.";
    public const string FileUploadFailed = "File upload failed";

    //Posts
    public const string MaxTagsItemError = "A maximum of {0} tags are allowed.";
    public const string PostNotFound = "The post not found";

    //Theme
    public const string ThemeNotFound = "The theme not found";
    public const string ThemeNameConflict = "The theme \"{0}\" already exists";
}