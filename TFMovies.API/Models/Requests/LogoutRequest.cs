using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class LogoutRequest
{  
    [Required]
    public string RefreshToken { get; set; }
}
