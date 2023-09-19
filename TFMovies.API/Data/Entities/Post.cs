using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TFMovies.API.Data.Entitiesl;

namespace TFMovies.API.Data.Entities;

public class Post : BaseModel
{
    [ForeignKey("User")]
    public string UserId { get; set; }

    [ForeignKey("Theme")]
    public string ThemeId { get; set; }

    [MaxLength(100)]
    public string Title { get; set; }
    public string HtmlContent { get; set; }

    [Url]
    public string CoverImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public User User { get; set; }
    public Theme Theme { get; set; }
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
}
