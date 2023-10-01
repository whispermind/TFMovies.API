using TFMovies.API.Models.Dto;

namespace TFMovies.API.Mappers;

public static class GenericMapper
{
    public static PagedResult<T> GetEmptyPagedResult<T>()
    {
        return new PagedResult<T>
        {
            Page = 0,
            Limit = 0,
            TotalPages = 0,
            TotalRecords = 0,
            Data = new List<T>()
        };
    }
}
