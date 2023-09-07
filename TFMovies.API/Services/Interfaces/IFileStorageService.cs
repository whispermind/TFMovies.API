using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IFileStorageService
{
    public Task<UploadFileResponse> UploadImageAsync(IFormFile file);
}
