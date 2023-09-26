namespace TFMovies.API.Models.Dto;

public class PostsQueryDto
{
    public IEnumerable<string>? Query { get; set; }
    public IEnumerable<string>? MatchingTagIdsQuery { get; set; }
    public IEnumerable<string>? MatchingCommentIdsQuery { get; set; }
}
