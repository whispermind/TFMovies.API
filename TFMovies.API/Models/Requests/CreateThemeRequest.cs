using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class CreateThemeRequest
{
    [Required]
    public string Name { get; set; }
}
