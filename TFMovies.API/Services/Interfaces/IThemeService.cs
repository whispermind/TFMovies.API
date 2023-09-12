using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IThemeService
{
    public Task CreateAsync(string name);
    public Task<IEnumerable<ThemeResponse>> GetAllAsync();
    public Task<ThemeResponse> GetByNameAsync(string name);
    public Task UpdateAsync(ThemeUpdateRequest model);
    public Task DeleteAsync(string id);
}
