namespace TFMovies.API.Models.Requests;

public class PostsQueryParams
{
    public string? Query { get; set; }
    public string? TagQuery { get; set; }
    public string? CommentQuery { get; set; }
}
