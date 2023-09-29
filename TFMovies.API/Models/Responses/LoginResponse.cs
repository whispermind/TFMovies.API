using TFMovies.API.Models.Dto;

namespace TFMovies.API.Models.Responses;

public class LoginResponse
{
    public UserShortInfoDto CurrentUser { get; set; }        
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}    