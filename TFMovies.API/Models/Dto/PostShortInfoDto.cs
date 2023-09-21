namespace TFMovies.API.Models.Dto;

public class PostShortInfoDto
{
    public string Id { get; set; }
    public string CoverImageUrl { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Author { get; set; }
    public bool IsLiked { get; set; } = false;

    public IEnumerable<string>? Tags { get; set; }
}
