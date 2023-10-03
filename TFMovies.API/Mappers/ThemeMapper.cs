using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;

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
}
