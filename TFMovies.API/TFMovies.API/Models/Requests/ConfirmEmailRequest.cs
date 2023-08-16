using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class ConfirmEmailRequest
{
    [Required]
    public string Token { get; set; }    
}
