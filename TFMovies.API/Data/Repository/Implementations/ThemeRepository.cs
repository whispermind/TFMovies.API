using Microsoft.EntityFrameworkCore;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public class ThemeRepository : BaseRepository<Theme>, IThemeRepository
{
    public ThemeRepository(DataContext context) : base(context)
    { }

    public async Task<Theme?> FindByNameAsync(string name)
    {
        var result = await _entities
             .FirstOrDefaultAsync(item =>
                 item.Name == name);

        return result;
    }
}
