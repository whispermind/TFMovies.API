using TFMovies.API.Data.Entities;

namespace TFMovies.API.Repositories.Interfaces;

public interface IThemeRepository : IBaseRepository<Theme>
{
    public Task<Theme?> FindByNameAsync(string name);
}
