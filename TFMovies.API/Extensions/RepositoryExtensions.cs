using System.Linq.Expressions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;

namespace TFMovies.API.Extensions;

public static class RepositoryExtensions
{
    public static PagingSortDto<T> ToPagingDto<T>(
        this PagingSortParams model,
        Expression<Func<T, object>>? sortSelector = null)      
    {
        return new PagingSortDto<T>
        {
            Page = model.Page,
            Limit = model.Limit,
            Order = model.Order,
            SortSelector = sortSelector            
        };
    }
}
