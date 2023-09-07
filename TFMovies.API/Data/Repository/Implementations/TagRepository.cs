using Microsoft.EntityFrameworkCore;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    public TagRepository(DataContext context) : base(context)
    { }

    public async Task<Tag?> FindByNameAsync(string name)
    {
        var result = await _entities
             .FirstOrDefaultAsync(item =>
                 item.Name == name);

        return result;
    }

    public async Task<IEnumerable<Tag>> FindByNamesAsync(IEnumerable<string> tagNames)
    {
        return await _entities
                    .Where(t => tagNames.Contains(t.Name))
                    .ToListAsync();
    }
}
