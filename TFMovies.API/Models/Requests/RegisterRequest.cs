using System.ComponentModel.DataAnnotations;
using TFMovies.API.Common.Constants;

namespace TFMovies.API.Models.Requests;

public class RegisterRequest
{
    /// <example>Jonny</example>
    [Required]
    [RegularExpression(UserRegulars.Nickname, ErrorMessage = ErrorMessages.IncorrectNickName)]    
    public string Nickname { get; set; }    

    [Required]
    [EmailAddress]
    [RegularExpression(UserRegulars.EmailPattern, ErrorMessage = ErrorMessages.EmailInvalidFormat)]
    public string Email { get; set; }

    /// <example>34Jvqt+K</example>
    [Required]
    [DataType(DataType.Password)]
    [RegularExpression(UserRegulars.Password, ErrorMessage = ErrorMessages.IncorrectPasswordComplexity)]
    public string Password { get; set; }

    /// <example>34Jvqt+K</example>
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
}
