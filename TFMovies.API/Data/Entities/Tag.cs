using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Data.Entities;

public class Tag
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Name { get; set; }

    public ICollection<PostTag>? PostTags { get; set; }
}
