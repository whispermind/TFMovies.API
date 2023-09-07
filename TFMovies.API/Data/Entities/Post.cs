using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Data.Entities;

public class Post
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [ForeignKey("User")]
    public string UserId { get; set; }

    [Required]
    [ForeignKey("Theme")]
    public string ThemeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; }

    [Required]
    public string HtmlContent { get; set; }

    [Required]
    [Url]
    public string CoverImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public User User { get; set; }
    public Theme Theme { get; set; }
    public ICollection<PostTag>? PostTags { get; set; }
}
