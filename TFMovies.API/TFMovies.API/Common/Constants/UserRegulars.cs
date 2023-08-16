namespace TFMovies.API.Common.Constants;

public static class UserRegulars
{
    public const string Nickname = @"^[A-Z][a-zA-Z]{1,}$";

    public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-_+=])[A-Za-z\d-_+=]{8,}$";
}
