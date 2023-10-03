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

    public static PagedResult<TDto> ToPaginatedResponse<TEntity, TDto>(
        PagedResult<TEntity> pagedData,
        Func<TEntity, TDto> transformFunc)
    {
        var data = pagedData.Data.Select(transformFunc).ToList();

        var result = new PagedResult<TDto>
        {
            Page = pagedData.Page,
            Limit = pagedData.Limit,
            TotalPages = pagedData.TotalPages,
            TotalRecords = pagedData.TotalRecords,
            Data = data
        };

        return result;        
    }
}
