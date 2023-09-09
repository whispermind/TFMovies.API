using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class ThemeCreateRequest
{
    [Required]
    public string Name { get; set; }
}
