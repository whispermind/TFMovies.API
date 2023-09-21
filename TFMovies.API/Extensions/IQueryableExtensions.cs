using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TFMovies.API.Filters;
using TFMovies.API.Models.Dto;

namespace TFMovies.API.Extensions;

public static class IQueryableExtensions
{
    public static async Task<PagedResult<T>> GetPagedDataAsync<T>(
        this IQueryable<T> query,
        PaginationFilter paginationFilter,
        Expression<Func<T, object>> sortSelector,
        string sortOrder)
    {
        var totalRecords = await query.CountAsync();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)paginationFilter.Limit);

        if (paginationFilter.Page > totalPages)
        {
            paginationFilter.Page = totalPages;
        }

        if (string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase))
        {
            query = query.OrderByDescending(sortSelector);
        }
        else
        {
            query = query.OrderBy(sortSelector);
        }

        query = query.Skip((paginationFilter.Page - 1) * paginationFilter.Limit)
                     .Take(paginationFilter.Limit);

        var data = await query.ToListAsync();

        return new PagedResult<T>
        {
            Page = paginationFilter.Page,
            Limit = paginationFilter.Limit,
            TotalPages = totalPages,
            TotalRecords = totalRecords,
            Data = data
        };
    }
}
