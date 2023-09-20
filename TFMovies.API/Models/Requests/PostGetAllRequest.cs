namespace TFMovies.API.Models.Requests;

public class PostGetAllRequest
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public string? Sort { get; set; }
    public string? ThemeId { get; set; }
}
