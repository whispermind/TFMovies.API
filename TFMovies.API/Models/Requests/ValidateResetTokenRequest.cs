using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class ValidateResetTokenRequest
{
    [Required]
    public string Token { get; set; }
}
