using Azure;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Repositories.Interfaces;

public interface IPostTagRepository : IBaseRepository<PostTag>
{
    public Task<List<PostTag>?> FindByPostIdAsync(string postId);
}
