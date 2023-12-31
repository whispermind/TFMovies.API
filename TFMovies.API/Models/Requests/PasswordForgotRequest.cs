﻿using System.ComponentModel.DataAnnotations;
using TFMovies.API.Common.Constants;

namespace TFMovies.API.Models.Requests;

public class PasswordForgotRequest
{
    [Required]
    [EmailAddress]
    [RegularExpression(UserRegulars.EmailPattern, ErrorMessage = ErrorMessages.EmailInvalidFormat)]
    public string Email { get; set; }
}
