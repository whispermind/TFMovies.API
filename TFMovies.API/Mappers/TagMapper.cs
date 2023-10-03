using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;

namespace TFMovies.API.Mappers;

public class TagMapper
{
    public static TagDto ToTagDto(Tag tag)
    {
        var result = new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };

        return result;
    }
}
