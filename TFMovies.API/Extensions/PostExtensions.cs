using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;

namespace TFMovies.API.Extensions;

public static class PostExtensions
{
    public static IEnumerable<TagDto> ToTagDtos(this Post post)
    {
        var result = post.PostTags?.Select(pt => new TagDto
        {
            Id = pt.Tag.Id,
            Name = pt.Tag.Name
        }).ToList() ?? Enumerable.Empty<TagDto>();

        return result;
    }    
}
