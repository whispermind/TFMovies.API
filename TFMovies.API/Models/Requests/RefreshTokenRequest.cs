﻿using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; }
}

