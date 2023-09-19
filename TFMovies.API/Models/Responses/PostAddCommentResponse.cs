namespace TFMovies.API.Models.Responses;

public class PostAddCommentResponse
{
    public string PostId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Author { get; set; }
}
