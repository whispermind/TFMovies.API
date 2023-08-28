using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class LinkWithTokenRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
