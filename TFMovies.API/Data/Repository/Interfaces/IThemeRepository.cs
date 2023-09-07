using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data.Repository.Interfaces;

public interface IThemeRepository : IBaseRepository<Theme>
{
    public Task<Theme?> FindByNameAsync(string name);
}
