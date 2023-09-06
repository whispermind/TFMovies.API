namespace TFMovies.API.Common.Constants;

public static class UserRegulars
{
    public const string Nickname = @"^[A-Z][a-zA-Z]{1,16}$";

    public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-_+=])[A-Za-z\d-_+=]{8,}$";

    public const string EmailPattern = @"^[\w-+.]+@([\w-]+\.)+[\w-]{2,4}$";
}
