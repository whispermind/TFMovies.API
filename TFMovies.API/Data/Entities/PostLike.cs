using System.ComponentModel.DataAnnotations.Schema;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data.Entitiesl;

public class PostLike : BaseModel
{
    [ForeignKey("Post")]
    public string PostId { get; set; }

    [ForeignKey("User")]
    public string? UserId { get; set; }

    public Post Post { get; set; }
    public User? User { get; set; }
}
