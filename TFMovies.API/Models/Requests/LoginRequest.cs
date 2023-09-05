using System.ComponentModel.DataAnnotations;
using TFMovies.API.Common.Constants;

namespace TFMovies.API.Models.Requests;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [RegularExpression(UserRegulars.EmailPattern, ErrorMessage = ErrorMessages.EmailInvalidFormat)]
    public string Email { get; set; }

    /// <example>34Jvqt+K</example>
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
