using System.ComponentModel.DataAnnotations;
using TFMovies.API.Common.Constants;

namespace TFMovies.API.Models.Requests;

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Token { get; set; }

    /// <example>34Jvqt+K</example>
    [Required]
    [DataType(DataType.Password)]
    [RegularExpression(UserRegulars.Password, ErrorMessage = ErrorMessages.IncorrectPasswordComplexity)]
    public string NewPassword { get; set; }

    /// <example>34Jvqt+K</example>
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; }
}
