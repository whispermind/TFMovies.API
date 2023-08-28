using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class VerifySecretTokenRequest
{
    [Required]
    public string Token { get; set; }
}
