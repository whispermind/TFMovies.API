using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class VerifyEmailRequest
{
    [Required]
    public string Token { get; set; }
}
