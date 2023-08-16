using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string AccessToken { get; set; }

    [Required]
    public string RefreshToken { get; set; }
}

