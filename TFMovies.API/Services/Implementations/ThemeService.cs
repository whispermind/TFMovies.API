using System.Net;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Services.Implementations;

public class ThemeService : IThemeService
{
    private readonly IThemeRepository _themeRepository;

    public ThemeService(IThemeRepository themeRepository)
    {
        _themeRepository = themeRepository;
    }

    public async Task CreateAsync(string name)
    {
        var theme = await _themeRepository.FindByNameAsync(name) ?? new Theme { Name = name };

        await _themeRepository.CreateAsync(theme);
    }

    public async Task DeleteAsync(string name)
    {
        var themeDb = await _themeRepository.FindByNameAsync(name);

        await _themeRepository.DeleteAsync(themeDb);
    }

    public async Task<IEnumerable<ThemeResponse>> GetAllAsync()
    {
        var themesDb = await _themeRepository.GetAllAsync();

        var themesResponse = themesDb.Select(theme =>
            new ThemeResponse
            {
                Name = theme.Name
            });

        return themesResponse;
    }

    public async Task<Theme> GetByNameAsync(string name)
    {
        var themeDb = await _themeRepository.FindByNameAsync(name);

        return themeDb;
    }

    public async Task UpdateAsync(UpdateThemeRequest model)
    {
        var themeDb = await FindByNameAsync(model.OldName);

        themeDb.Name = model.NewName;
        await _themeRepository.UpdateAsync(themeDb);
    }

    private async Task<Theme> FindByNameAsync(string name)
    {
        var themeDb = await _themeRepository.FindByNameAsync(name);

        if (themeDb == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, string.Format(ErrorMessages.ThemeNotFound, name));
        }

        return themeDb;
    }
}
