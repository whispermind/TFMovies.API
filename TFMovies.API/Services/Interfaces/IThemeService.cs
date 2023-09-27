using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;

namespace TFMovies.API.Services.Interfaces;

public interface IThemeService
{
    public Task CreateAsync(string name);
    public Task<IEnumerable<ThemeDto>> GetAllAsync();
    public Task<ThemeDto> GetByNameAsync(string name);
    public Task UpdateAsync(ThemeUpdateRequest model);
    public Task DeleteAsync(string id);
}
