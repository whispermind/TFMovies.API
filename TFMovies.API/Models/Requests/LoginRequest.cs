using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    /// <example>34Jvqt+K</example>
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
