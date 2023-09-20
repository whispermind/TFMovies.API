using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class PostAddCommentRequest
{
    [Required]
    public string Content { get; set; }
}
