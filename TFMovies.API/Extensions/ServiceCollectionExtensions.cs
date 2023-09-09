using TFMovies.API.Integrations;
using TFMovies.API.Models.Dto;

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
