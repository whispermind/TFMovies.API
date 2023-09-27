namespace TFMovies.API.Models.Dto;

public class PostByAuthorDto
{
    public string Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<TagDto>? Tags { get; set; }
}
