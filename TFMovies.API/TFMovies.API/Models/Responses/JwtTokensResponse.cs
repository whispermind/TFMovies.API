namespace TFMovies.API.Models.Responses;

public class JwtTokensResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
