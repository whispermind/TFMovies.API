using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Models.Requests;

public class FileUploadRequest
{
    [Required]
    public IFormFile File { get; init; }
}
