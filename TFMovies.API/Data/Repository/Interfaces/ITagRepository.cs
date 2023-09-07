using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data.Repository.Interfaces;

public interface ITagRepository : IBaseRepository<Tag>
{
    public Task<Tag?> FindByNameAsync(string name);

    public Task<IEnumerable<Tag>> FindByNamesAsync(IEnumerable<string> tagNames);
}
