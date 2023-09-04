namespace TFMovies.API.Models.Responses;

public class LoginResponse
{   
    public string? Nickname { get; set; }
    public string? Role { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
