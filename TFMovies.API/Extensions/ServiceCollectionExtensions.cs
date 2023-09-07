using TFMovies.API.Models.Dto;
using TFMovies.API.Services.Implementations;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileStorageService(this IServiceCollection services, string connectionString)
    {
        services.Configure<BlobSettings>(options => options.ConnectionString = connectionString);
        services.AddScoped<IFileStorageService, BlobStorageService>();

        return services;
    }
}
