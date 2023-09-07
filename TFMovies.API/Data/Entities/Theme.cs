using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Data.Entities;

public class Theme
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    public ICollection<Post>? Posts { get; set; }
}
