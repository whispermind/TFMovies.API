using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class UploadFileRequest
{
    [Required]
    public IFormFile File { get; init; }
}
