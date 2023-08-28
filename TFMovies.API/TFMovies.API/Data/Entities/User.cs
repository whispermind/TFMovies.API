using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Data.Entities;

public class User : IdentityUser
{
    [Required]
    public string Nickname { get; set; }
    public ICollection<UserActionToken>? UserActionTokens { get; set; }
    public ICollection<RefreshToken>? RefreshTokens { get; set; }
}
