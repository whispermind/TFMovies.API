using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFMovies.API.Data.Entities;

[Index(nameof(PostId), nameof(TagId), IsUnique = true)]
public class PostTag : BaseModel
{
    [ForeignKey("Post")]
    public string PostId { get; set; }

    [ForeignKey("Tag")]
    public string TagId { get; set; }

    public Post Post { get; set; }
    public Tag Tag { get; set; }
}
