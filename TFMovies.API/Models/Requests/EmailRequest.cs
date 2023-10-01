namespace TFMovies.API.Models.Requests;

public class EmailRequest
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
}
