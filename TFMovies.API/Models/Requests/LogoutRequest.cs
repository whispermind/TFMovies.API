using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class LogoutRequest
{    
    public string AccessToken { get; set; }

    [Required]
    public string RefreshToken { get; set; }
}
