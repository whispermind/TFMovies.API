using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;

namespace TFMovies.API.Mappers;

public static class ThemeMapper
{
    public static ThemeDto ToThemeDto(Post post)
    {
        var result = new ThemeDto
        {
            Id = post.ThemeId,
            Name = post.Theme.Name
        };

        return result;
    }
    public static ThemeDto ToThemeDto(Theme theme)
    {
        return new ThemeDto
        {
            Id = theme.Id,
            Name = theme.Name
        };
    }

    public static Theme ToTheme(ThemeDto themeDto)
    {
        return new Theme
        {
            Id = themeDto.Id,
            Name = themeDto.Name
        };
    }    
}
