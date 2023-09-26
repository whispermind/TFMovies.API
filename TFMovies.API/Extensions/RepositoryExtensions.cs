using System.Linq.Expressions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;

namespace TFMovies.API.Extensions;

public static class RepositoryExtensions
{
    public static PagingSortFilterDto<T> ToPagingDto<T>(
        this PagingSortFilterParams model,
        Expression<Func<T, object>>? sortSelector = null,
        Expression<Func<T, bool>>? filterPredicate = null)
    {
        return new PagingSortFilterDto<T>
        {
            Page = model.Page,
            Limit = model.Limit,
            Order = model.Order,
            SortSelector = sortSelector,
            FilterPredicate = filterPredicate
        };
    }
}
