using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class EmailVerifyRequest
{
    [Required]
    public string Token { get; set; }
}
