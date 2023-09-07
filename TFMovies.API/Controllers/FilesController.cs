using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;

[Route("files")]
[ApiController]
[Produces("application/json")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public FilesController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Uploads an image file.
    /// </summary>
    /// <param name="file">The image file to be uploaded.</param> 
    /// <returns>Returns FileUrl if successful, otherwise returns an appropriate error status.</returns>  
    [HttpPost("upload-image")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(UploadFileResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> UploadImageAsync([FromForm] UploadFileRequest file)
    {
        var result = await _fileStorageService.UploadImageAsync(file.File);

        return Ok(result);
    }
}
