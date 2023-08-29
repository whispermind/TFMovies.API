using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class RefreshTokenRequest
{   
    public string RefreshToken { get; set; }
}

