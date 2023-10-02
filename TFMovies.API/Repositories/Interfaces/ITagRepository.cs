using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Requests;

namespace TFMovies.API.Repositories.Interfaces;

public interface ITagRepository : IBaseRepository<Tag>
{
    public Task<Tag> FindByNameAsync(string name);
    public Task<IEnumerable<Tag>> FindByNamesAsync(IEnumerable<string> tagNames);
    public Task<IEnumerable<Tag>> GetTagsAsync(PagingSortParams model);
}
