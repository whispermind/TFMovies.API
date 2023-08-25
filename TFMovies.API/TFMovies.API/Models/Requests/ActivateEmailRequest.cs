using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class ActivateEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
