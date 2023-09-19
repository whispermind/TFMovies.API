using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class PostAddCommentRequest
{
    [Required]
    public string PostId { get; set; }

    [Required]
    public string Content { get; set; }
}
