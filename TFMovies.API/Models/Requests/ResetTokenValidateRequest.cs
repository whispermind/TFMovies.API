using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class ResetTokenValidateRequest
{
    [Required]
    public string Token { get; set; }
}
