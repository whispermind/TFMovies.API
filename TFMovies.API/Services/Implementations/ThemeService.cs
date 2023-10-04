using System.Net;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Exceptions;
using TFMovies.API.Mappers;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Repositories.Interfaces;
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

    public async Task DeleteAsync(string id)
    {
        await _themeRepository.DeleteByIdAsync(id);
    }

    public async Task<IEnumerable<ThemeDto>> GetAllAsync()
    {
        var themesDb = await _themeRepository.GetAllAsync();

        var themesResponse = themesDb.Select(ThemeMapper.ToThemeDto);

        return themesResponse;
    }  

    public async Task UpdateAsync(ThemeUpdateRequest model)
    {
        var theme = await GetByNameAsync(model.OldName);

        if (theme == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, string.Format(ErrorMessages.ThemeNotFoundWithName, model.OldName));
        }

        theme.Name = model.NewName;

        await _themeRepository.SaveChangesAsync();
    }

    public async Task<ThemeDto> GetByNameAsync(string name)
    {
        var themeDb = await _themeRepository.FindByNameAsync(name);

        if (themeDb == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, string.Format(ErrorMessages.ThemeNotFoundWithName, name));
        }

        return ThemeMapper.ToThemeDto(themeDb);
    }
}
