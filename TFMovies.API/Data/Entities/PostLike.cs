using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFMovies.API.Data.Entities;

[Index(nameof(PostId), nameof(UserId), IsUnique = true)]
public class PostLike : BaseModel
{
    [ForeignKey("Post")]
    public string PostId { get; set; }

    [ForeignKey("User")]
    public string UserId { get; set; }

    public Post Post { get; set; }
    public User User { get; set; }
}
