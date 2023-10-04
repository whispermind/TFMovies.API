using System;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Common.Constants;

public static class ErrorMessages
{
    //Common
    public const string UnexpectedError = "An unexpected error has occurred. Please try again later.";
    public const string OperationFailed = "Something went wrong. Please try again later.";
    public const string GenerateUniqueTokenFailed = "Unable to generate a unique action token after multiple attempts.";
    public const string SearchColumnsNotDefined = "Search Columns doesn't defined in derived repository.";
    public const string SearchFailedNoValuesProvided = "No search values provided. Please enter a value to proceed with the search.";    

    //User 
    //Model State -registration
    public const string UserAlreadyExists = "A user with the provided email already exists.";
    public const string EmailInvalidFormat = "The email address is not in a valid format. Please enter a valid email address.";
    public const string InvalidToken = "Invalid token.";
    public const string IncorrectNickName = "{0} must start with an uppercase letter and contain only Latin letters, minimum 2 symbols.";
    public const string IncorrectPasswordComplexity = "The Password must be at least 8 characters long and contain only Latin letters, at least one uppercase letter, one lowercase letter, one number, and one special character like -, _, +, =.";
    //other
    public const string LoginFailed = "Login failed. Invalid credentials being provided.";
    public const string UserNotFound = "User not found.";
    public const string UnconfirmedEmail = "Email address has not been confirmed yet. Please check your inbox for the confirmation link.";
    public const string InvalidRole = "Invalid Role specified.";
    public const string UpdateRoleFailed = "Error updating user role";    
    public const string UserMissingRoleError = "The user does not have an assigned role.";  
    public const string DeletedUserOperationError = "Operation not allowed on a deleted user.";
    public const string ChangeRequestAlreadyExists = "A role change request has already been submitted. Please wait for your request to be processed.";

    // Files exception
    public const string UploadedFileInvalid = "No file uploaded or the file name is invalid.";
    public const string FileUploadFailed = "File upload failed";
    public const string FileTypeNotAllowed = "File must be one of the following types: {0}";
    public const string FileSizeTooLarge = "File size should be less than {0} MB";

    //Posts
    public const string MaxTagsItemError = "A maximum of {0} tags are allowed.";
    public const string PostNotFound = "The post not found";
    public const string TitleMaxLengthError = "Title cannot be more than 250 characters.";
    public const string HtmlContentMaxLengthError = "Content cannot be more than 250,000 characters.";

    //Theme
    public const string ThemeNotFound = "The theme not found";
    public const string ThemeNameConflict = "The theme \"{0}\" already exists";
    public const string ThemeNotFoundWithName = "Theme with the name {0} not found.";

    //Comment
    public const string CommentNotFound = "The comment not found";
    public const string CommentDeleteForbidden = "Permission to delete this comment is denied.";
}