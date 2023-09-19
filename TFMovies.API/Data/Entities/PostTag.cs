using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFMovies.API.Data.Entities;

[PrimaryKey(nameof(PostId), nameof(TagId))]
public class PostTag
{
    [ForeignKey("Post")]
    public string PostId { get; set; }

    [ForeignKey("Tag")]
    public string TagId { get; set; }

    public Post Post { get; set; }
    public Tag Tag { get; set; }
}
