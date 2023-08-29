﻿using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
