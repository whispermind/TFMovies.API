using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System.Net;
using TFMovies.API.Common.Constants;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Integrations;

public class BlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobSettings _blobSettings;

    public BlobStorageService(IOptions<BlobSettings> options)
    {
        _blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
        _blobSettings = options.Value;       
    }

    public async Task<FileUploadResponse> UploadImageAsync(IFormFile file)
    {
        var containerName = _blobSettings.Container;

        if (string.IsNullOrEmpty(containerName))
        {
            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed); //500
        }

        if (file == null || string.IsNullOrEmpty(file.FileName))
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.UploadedFileInvalid); //400
        }

        // Check file size
        var maxFileSizeMb = _blobSettings.ImageSettings.MaxSizeMb;

        if (file.Length > maxFileSizeMb * 1024 * 1024) 
        {
            throw new ServiceException(HttpStatusCode.BadRequest, string.Format(ErrorMessages.FileSizeTooLarge, maxFileSizeMb)); //400
        }

        // Check file format
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        var allowedExtensions = _blobSettings.ImageSettings.AllowedExtensions;

        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new ServiceException(HttpStatusCode.BadRequest, string.Format(ErrorMessages.FileTypeNotAllowed, string.Join(", ", allowedExtensions)));  //400
        }        

        var fileName = GenerateFileName(file.FileName);

        var response = await UploadFileAsync(file.OpenReadStream(), containerName, fileName);

        return new FileUploadResponse
        {
            FileUrl = response
        };
    }

    private async Task<string> UploadFileAsync(Stream fileStream, string containerName, string fileName)
    {
        await using (fileStream)
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            await blobContainerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);

            Response<BlobContentInfo> response = await blobClient.UploadAsync(fileStream);

            if (response.GetRawResponse().IsError)
            {
                throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.FileUploadFailed); //500
            }

            return blobClient.Uri.AbsoluteUri.ToString();
        }
    }

    private static string GenerateFileName(string originalFileName)
    {
        string newFileName = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

        newFileName += Path.GetExtension(originalFileName).ToLower();

        return newFileName;
    }
}
