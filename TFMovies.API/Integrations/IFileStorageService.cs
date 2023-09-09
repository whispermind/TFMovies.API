using TFMovies.API.Models.Responses;

namespace TFMovies.API.Integrations;

public interface IFileStorageService
{
    public Task<FileUploadResponse> UploadImageAsync(IFormFile file);
}
