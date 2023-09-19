using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class ThemeUpdateRequest
{
    [Required]
    public string OldName { get; set; }

    [Required]
    public string NewName { get; set; }
}
