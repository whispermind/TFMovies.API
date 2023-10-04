namespace TFMovies.API.Models.Dto;

public class CommentDetailDto
{
    public string Id { get; set; }
    public string AuthorId { get; set; }
    public string Author { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }    
}
